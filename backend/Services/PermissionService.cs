using Backend.Data;
using Backend.Models.DTO.Permissions;
using Backend.Models.Document;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Backend.Services;

public class PermissionService : IPermissionService
{
    private readonly DocumentDbContext _context;
    private readonly ILogger<PermissionService> _logger;

    public PermissionService(DocumentDbContext context, ILogger<PermissionService> logger)
    {
        _context = context;
        _logger = logger;
    }

        public async Task<bool> CanAccessFolderAsync(Guid userId, Guid folderId, string requiredPermission)
    {
        // System admins have full access
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            _logger?.LogDebug("PermissionService: user {UserId} not found.", userId);
        }
            if (user?.Role == "SystemAdmin")
            return true;

        var folder = await _context.Folders
            .Include(f => f.Owner)
            .FirstOrDefaultAsync(f => f.Id == folderId);

        if (folder == null)
        {
            _logger?.LogDebug("PermissionService: folder {FolderId} not found.", folderId);
            return false;
        }

            // Admin role also has full access (organization-wide admin)
            if (user?.Role == "Admin")
                return true;

            // Owner has full access
        if (folder.OwnerId == userId)
            return true;

            // If it's a system folder, treat read as available to all active users
            if (folder.IsSystemFolder)
            {
                if (requiredPermission == "Read")
                    return true;

                // Grant write permissions on system folders to roles that can create documents
                var rolesWithWrite = new[] { "SystemAdmin", "Admin", "Manager", "Editor", "Contributor" };
                if (requiredPermission == "Write" && rolesWithWrite.Contains(user?.Role))
                    return true;
            }

        // Check explicit permissions
        var permission = await _context.Permissions
            .FirstOrDefaultAsync(p => p.UserId == userId && p.FolderId == folderId);

        if (permission == null)
            return false;

        return requiredPermission switch
        {
            "Read" => permission.PermissionType is "Read" or "Write" or "Admin",
            "Write" => permission.PermissionType is "Write" or "Admin",
            "Admin" => permission.PermissionType == "Admin",
            _ => false
        };
    }

        public async Task<bool> CanAccessDocumentAsync(Guid userId, Guid documentId, string requiredPermission)
    {
        // System admins have full access
        var user = await _context.Users.FindAsync(userId);
            if (user?.Role == "SystemAdmin" || user?.Role == "Admin")
            return true;

        var document = await _context.Documents
            .Include(d => d.Owner)
            .Include(d => d.Folder)
            .FirstOrDefaultAsync(d => d.Id == documentId);

        if (document == null)
            return false;

        // Owner has full access
        if (document.OwnerId == userId)
            return true;

        // Check folder permissions first (inherited)
        if (await CanAccessFolderAsync(userId, document.FolderId, requiredPermission))
            return true;

        // Check explicit document permissions
        var permission = await _context.Permissions
            .FirstOrDefaultAsync(p => p.UserId == userId && p.DocumentId == documentId);

        if (permission == null)
            return false;

        return requiredPermission switch
        {
            "Read" => permission.PermissionType is "Read" or "Write" or "Admin",
            "Write" => permission.PermissionType is "Write" or "Admin",
            "Admin" => permission.PermissionType == "Admin",
            _ => false
        };
    }

    public async Task<PermissionDTO?> GrantPermissionAsync(CreatePermissionRequest request, string grantedBy)
    {
        // Validate that either FolderId or DocumentId is provided
        if (!request.FolderId.HasValue && !request.DocumentId.HasValue)
            return null;

        // Check if permission already exists
        var existingPermission = await _context.Permissions
            .FirstOrDefaultAsync(p => p.UserId == request.UserId &&
                                     p.FolderId == request.FolderId &&
                                     p.DocumentId == request.DocumentId);

        if (existingPermission != null)
        {
            // Update existing permission
            existingPermission.PermissionType = request.PermissionType;
            existingPermission.ModifiedAt = DateTime.UtcNow;
            existingPermission.ModifiedBy = grantedBy;
        }
        else
        {
            // Create new permission
            existingPermission = new Permission
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                FolderId = request.FolderId,
                DocumentId = request.DocumentId,
                PermissionType = request.PermissionType,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = grantedBy
            };
            _context.Permissions.Add(existingPermission);
        }

        await _context.SaveChangesAsync();

        return await GetPermissionDTOAsync(existingPermission.Id);
    }

    public async Task<bool> RevokePermissionAsync(Guid permissionId)
    {
        var permission = await _context.Permissions.FindAsync(permissionId);
        if (permission == null)
            return false;

        _context.Permissions.Remove(permission);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<PermissionDTO>> GetUserPermissionsAsync(Guid userId)
    {
        return await _context.Permissions
            .Where(p => p.UserId == userId)
            .Include(p => p.User)
            .Include(p => p.Folder)
            .Include(p => p.Document)
            .Select(p => new PermissionDTO
            {
                Id = p.Id,
                UserId = p.UserId,
                UserName = p.User.Username,
                FolderId = p.FolderId,
                FolderPath = p.Folder != null ? p.Folder.Path : null,
                DocumentId = p.DocumentId,
                DocumentTitle = p.Document != null ? p.Document.Title : null,
                PermissionType = p.PermissionType,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<List<PermissionDTO>> GetFolderPermissionsAsync(Guid folderId)
    {
        return await _context.Permissions
            .Where(p => p.FolderId == folderId)
            .Include(p => p.User)
            .Include(p => p.Folder)
            .Select(p => new PermissionDTO
            {
                Id = p.Id,
                UserId = p.UserId,
                UserName = p.User.Username,
                FolderId = p.FolderId,
                FolderPath = p.Folder!.Path,
                DocumentId = null,
                DocumentTitle = null,
                PermissionType = p.PermissionType,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<List<PermissionDTO>> GetDocumentPermissionsAsync(Guid documentId)
    {
        return await _context.Permissions
            .Where(p => p.DocumentId == documentId)
            .Include(p => p.User)
            .Include(p => p.Document)
            .Select(p => new PermissionDTO
            {
                Id = p.Id,
                UserId = p.UserId,
                UserName = p.User.Username,
                FolderId = null,
                FolderPath = null,
                DocumentId = p.DocumentId,
                DocumentTitle = p.Document!.Title,
                PermissionType = p.PermissionType,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();
    }

    private async Task<PermissionDTO?> GetPermissionDTOAsync(Guid permissionId)
    {
        return await _context.Permissions
            .Where(p => p.Id == permissionId)
            .Include(p => p.User)
            .Include(p => p.Folder)
            .Include(p => p.Document)
            .Select(p => new PermissionDTO
            {
                Id = p.Id,
                UserId = p.UserId,
                UserName = p.User.Username,
                FolderId = p.FolderId,
                FolderPath = p.Folder != null ? p.Folder.Path : null,
                DocumentId = p.DocumentId,
                DocumentTitle = p.Document != null ? p.Document.Title : null,
                PermissionType = p.PermissionType,
                CreatedAt = p.CreatedAt
            })
            .FirstOrDefaultAsync();
    }
}
