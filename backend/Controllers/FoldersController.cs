using Backend.Data;
using Backend.Models.DTO.Common;
using Backend.Models.DTO.Folders;
using Backend.Models.Document;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Backend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class FoldersController : ControllerBase
{
    private readonly EdmDbContext _context;
    private readonly IPermissionService _permissionService;

    public FoldersController(EdmDbContext context, IPermissionService permissionService)
    {
        _context = context;
        _permissionService = permissionService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<FolderDTO>>>> GetFolders([FromQuery] Guid? parentFolderId)
    {
        var userId = GetCurrentUserId();
        var query = _context.Folders.AsQueryable();

        if (parentFolderId.HasValue)
            query = query.Where(f => f.ParentFolderId == parentFolderId);
        else
            query = query.Where(f => f.ParentFolderId == null);

        var folders = await query
            .Include(f => f.Owner)
            .Include(f => f.SubFolders)
            .Include(f => f.Documents)
            .Select(f => new FolderDTO
            {
                Id = f.Id,
                Name = f.Name,
                Description = f.Description,
                Path = f.Path,
                Level = f.Level,
                ParentFolderId = f.ParentFolderId,
                ParentFolderName = f.ParentFolder != null ? f.ParentFolder.Name : null,
                IsSystemFolder = f.IsSystemFolder,
                OwnerId = f.OwnerId,
                OwnerName = f.Owner.Username,
                CreatedAt = f.CreatedAt,
                SubFolderCount = f.SubFolders.Count,
                DocumentCount = f.Documents.Count
            })
            .ToListAsync();

        // Filter by permissions
        var accessibleFolders = new List<FolderDTO>();
        foreach (var folder in folders)
        {
            if (await _permissionService.CanAccessFolderAsync(userId, folder.Id, "Read"))
                accessibleFolders.Add(folder);
        }

        return Ok(ApiResponse<List<FolderDTO>>.SuccessResponse(accessibleFolders));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<FolderDTO>>> GetFolder(Guid id)
    {
        var userId = GetCurrentUserId();

        if (!await _permissionService.CanAccessFolderAsync(userId, id, "Read"))
            return Forbid();

        var folder = await _context.Folders
            .Include(f => f.Owner)
            .Include(f => f.SubFolders)
            .Include(f => f.Documents)
            .Where(f => f.Id == id)
            .Select(f => new FolderDTO
            {
                Id = f.Id,
                Name = f.Name,
                Description = f.Description,
                Path = f.Path,
                Level = f.Level,
                ParentFolderId = f.ParentFolderId,
                ParentFolderName = f.ParentFolder != null ? f.ParentFolder.Name : null,
                IsSystemFolder = f.IsSystemFolder,
                OwnerId = f.OwnerId,
                OwnerName = f.Owner.Username,
                CreatedAt = f.CreatedAt,
                SubFolderCount = f.SubFolders.Count,
                DocumentCount = f.Documents.Count
            })
            .FirstOrDefaultAsync();

        if (folder == null)
            return NotFound(ApiResponse<FolderDTO>.ErrorResponse("Folder not found"));

        return Ok(ApiResponse<FolderDTO>.SuccessResponse(folder));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<FolderDTO>>> CreateFolder([FromBody] CreateFolderRequest request)
    {
        var userId = GetCurrentUserId();
        var username = GetCurrentUsername();

        // Check parent folder access if specified
        if (request.ParentFolderId.HasValue)
        {
            if (!await _permissionService.CanAccessFolderAsync(userId, request.ParentFolderId.Value, "Write"))
                return Forbid();
        }

        var parentFolder = request.ParentFolderId.HasValue
            ? await _context.Folders.FindAsync(request.ParentFolderId.Value)
            : await _context.Folders.FirstOrDefaultAsync(f => f.ParentFolderId == null);

        var folder = new Folder
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            ParentFolderId = request.ParentFolderId,
            Path = parentFolder != null ? $"{parentFolder.Path}{request.Name}/" : $"/{request.Name}/",
            Level = parentFolder != null ? parentFolder.Level + 1 : 0,
            IsSystemFolder = false,
            OwnerId = userId,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = username
        };

        _context.Folders.Add(folder);
        await _context.SaveChangesAsync();

        var folderDto = new FolderDTO
        {
            Id = folder.Id,
            Name = folder.Name,
            Description = folder.Description,
            Path = folder.Path,
            Level = folder.Level,
            ParentFolderId = folder.ParentFolderId,
            IsSystemFolder = folder.IsSystemFolder,
            OwnerId = folder.OwnerId,
            OwnerName = username,
            CreatedAt = folder.CreatedAt,
            SubFolderCount = 0,
            DocumentCount = 0
        };

        return CreatedAtAction(nameof(GetFolder), new { id = folder.Id }, 
            ApiResponse<FolderDTO>.SuccessResponse(folderDto, "Folder created successfully"));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<FolderDTO>>> UpdateFolder(Guid id, [FromBody] UpdateFolderRequest request)
    {
        var userId = GetCurrentUserId();
        var username = GetCurrentUsername();

        if (!await _permissionService.CanAccessFolderAsync(userId, id, "Write"))
            return Forbid();

        var folder = await _context.Folders.FindAsync(id);
        if (folder == null || folder.IsDeleted)
            return NotFound(ApiResponse<FolderDTO>.ErrorResponse("Folder not found"));

        if (folder.IsSystemFolder)
            return BadRequest(ApiResponse<FolderDTO>.ErrorResponse("Cannot modify system folders"));

        folder.Name = request.Name;
        folder.Description = request.Description;
        folder.ModifiedAt = DateTime.UtcNow;
        folder.ModifiedBy = username;

        await _context.SaveChangesAsync();

        var folderDto = await GetFolder(id);
        return Ok(ApiResponse<FolderDTO>.SuccessResponse(folderDto.Value?.Data!, "Folder updated successfully"));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteFolder(Guid id)
    {
        var userId = GetCurrentUserId();
        var username = GetCurrentUsername();

        if (!await _permissionService.CanAccessFolderAsync(userId, id, "Admin"))
            return Forbid();

        var folder = await _context.Folders.FindAsync(id);
        if (folder == null || folder.IsDeleted)
            return NotFound(ApiResponse<bool>.ErrorResponse("Folder not found"));

        if (folder.IsSystemFolder)
            return BadRequest(ApiResponse<bool>.ErrorResponse("Cannot delete system folders"));

        // Soft delete
        folder.IsDeleted = true;
        folder.DeletedAt = DateTime.UtcNow;
        folder.DeletedBy = username;

        await _context.SaveChangesAsync();

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Folder deleted successfully"));
    }

    private Guid GetCurrentUserId()
    {
        return Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
    }

    private string GetCurrentUsername()
    {
        return User.Identity?.Name ?? "";
    }
}
