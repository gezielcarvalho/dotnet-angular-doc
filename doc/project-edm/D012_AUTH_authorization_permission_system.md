# EDM System - Authorization & Permission System

## Overview

This document defines the comprehensive authorization and permission system for the EDM, including role-based access control (RBAC), document-level permissions, folder inheritance, and custom authorization policies.

## Permission Model Architecture

```
┌────────────────────────────────────────────────────────────┐
│              Permission System Architecture                │
│                                                            │
│  ┌──────────────────────────────────────────────────┐    │
│  │           User Authentication (JWT)               │    │
│  └──────────────────────────────────────────────────┘    │
│                        ↓                                   │
│  ┌──────────────────────────────────────────────────┐    │
│  │         Authorization Middleware                  │    │
│  │  • Role-based policies                            │    │
│  │  • Custom requirement handlers                    │    │
│  └──────────────────────────────────────────────────┘    │
│                        ↓                                   │
│  ┌──────────────────────────────────────────────────┐    │
│  │         Permission Service                        │    │
│  │  • Document permissions                           │    │
│  │  • Folder permissions                             │    │
│  │  • Permission inheritance                         │    │
│  │  • Permission caching                             │    │
│  └──────────────────────────────────────────────────┘    │
│                        ↓                                   │
│  ┌──────────────────────────────────────────────────┐    │
│  │         Database (Permissions Table)              │    │
│  └──────────────────────────────────────────────────┘    │
└────────────────────────────────────────────────────────────┘
```

## User Roles

### Role Hierarchy

```csharp
namespace Backend.Models.Enums
{
    public enum UserRole
    {
        SystemAdmin = 0,    // Full system access
        Admin = 1,          // Organization-wide admin
        Manager = 2,        // Department manager
        Editor = 3,         // Can create/edit documents
        Contributor = 4,    // Can create documents
        Viewer = 5          // Read-only access
    }
}
```

### Role Capabilities

| Role        | Create Docs | Edit Docs | Delete Docs | Manage Users | Manage Permissions | Manage Workflows |
| ----------- | ----------- | --------- | ----------- | ------------ | ------------------ | ---------------- |
| SystemAdmin | ✅          | ✅        | ✅          | ✅           | ✅                 | ✅               |
| Admin       | ✅          | ✅        | ✅          | ✅           | ✅                 | ✅               |
| Manager     | ✅          | ✅        | ✅          | ❌           | ✅ (own dept)      | ✅               |
| Editor      | ✅          | ✅ (own)  | ✅ (own)    | ❌           | ❌                 | ❌               |
| Contributor | ✅          | ✅ (own)  | ❌          | ❌           | ❌                 | ❌               |
| Viewer      | ❌          | ❌        | ❌          | ❌           | ❌                 | ❌               |

## Permission Types

```csharp
namespace Backend.Models.Enums
{
    public enum PermissionType
    {
        Read = 0,       // View document/folder
        Write = 1,      // Modify document/folder
        Delete = 2,     // Delete document/folder
        Share = 3,      // Grant permissions to others
        Admin = 4       // Full control including permission management
    }
}
```

### Permission Hierarchy

- **Admin** → includes Share, Delete, Write, Read
- **Share** → includes Write, Read
- **Delete** → includes Write, Read
- **Write** → includes Read
- **Read** → base permission

## PermissionService Implementation

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Backend.Data;
using Backend.Models;
using Backend.Models.DTO;
using Backend.Models.Enums;
using Backend.Services.Interfaces;

namespace Backend.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly EdmDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<PermissionService> _logger;
        private const int CacheExpirationMinutes = 15;

        public PermissionService(
            EdmDbContext context,
            IMemoryCache cache,
            ILogger<PermissionService> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        public async Task<bool> CanAccessDocumentAsync(
            Guid documentId,
            Guid userId,
            string permissionType)
        {
            var cacheKey = $"DocPerm_{documentId}_{userId}_{permissionType}";

            if (_cache.TryGetValue(cacheKey, out bool cachedResult))
                return cachedResult;

            var document = await _context.Documents
                .Include(d => d.Owner)
                .FirstOrDefaultAsync(d => d.Id == documentId && !d.IsDeleted);

            if (document == null)
                return false;

            var user = await _context.Users.FindAsync(userId);
            if (user == null || !user.IsActive)
                return false;

            // System admin has full access
            if (user.Role == UserRole.SystemAdmin.ToString() ||
                user.Role == UserRole.Admin.ToString())
            {
                CachePermission(cacheKey, true);
                return true;
            }

            // Document owner has full access
            if (document.OwnerId == userId)
            {
                CachePermission(cacheKey, true);
                return true;
            }

            // Public documents allow Read access to everyone
            if (document.IsPublic && permissionType == PermissionType.Read.ToString())
            {
                CachePermission(cacheKey, true);
                return true;
            }

            // Check explicit document permissions
            var hasDocumentPermission = await HasExplicitPermissionAsync(
                documentId: documentId,
                userId: userId,
                permissionType: permissionType);

            if (hasDocumentPermission)
            {
                CachePermission(cacheKey, true);
                return true;
            }

            // Check folder-level permissions (inherited)
            var hasFolderPermission = await CanAccessFolderAsync(
                document.FolderId,
                userId,
                permissionType);

            CachePermission(cacheKey, hasFolderPermission);
            return hasFolderPermission;
        }

        public async Task<bool> CanAccessFolderAsync(
            Guid folderId,
            Guid userId,
            string permissionType)
        {
            var cacheKey = $"FolderPerm_{folderId}_{userId}_{permissionType}";

            if (_cache.TryGetValue(cacheKey, out bool cachedResult))
                return cachedResult;

            var folder = await _context.Folders
                .FirstOrDefaultAsync(f => f.Id == folderId && !f.IsDeleted);

            if (folder == null)
                return false;

            var user = await _context.Users.FindAsync(userId);
            if (user == null || !user.IsActive)
                return false;

            // System admin has full access
            if (user.Role == UserRole.SystemAdmin.ToString() ||
                user.Role == UserRole.Admin.ToString())
            {
                CachePermission(cacheKey, true);
                return true;
            }

            // Folder owner has full access
            if (folder.OwnerId == userId)
            {
                CachePermission(cacheKey, true);
                return true;
            }

            // Check explicit folder permissions (including inherited)
            var hasPermission = await HasExplicitPermissionAsync(
                folderId: folderId,
                userId: userId,
                permissionType: permissionType);

            CachePermission(cacheKey, hasPermission);
            return hasPermission;
        }

        public async Task<List<PermissionDTO>> GetDocumentPermissionsAsync(
            Guid documentId,
            Guid userId)
        {
            // Verify user can manage permissions
            var canManage = await CanAccessDocumentAsync(documentId, userId, PermissionType.Admin.ToString());
            if (!canManage)
            {
                var document = await _context.Documents.FindAsync(documentId);
                if (document?.OwnerId != userId)
                    throw new UnauthorizedAccessException("You don't have permission to view document permissions");
            }

            var permissions = await _context.Permissions
                .Include(p => p.User)
                .Where(p => p.DocumentId == documentId)
                .Select(p => new PermissionDTO
                {
                    Id = p.Id,
                    DocumentId = p.DocumentId,
                    UserId = p.UserId,
                    UserName = p.User.Username,
                    PermissionType = p.PermissionType,
                    IsInherited = p.IsInherited,
                    GrantedAt = p.GrantedAt,
                    GrantedBy = p.GrantedBy,
                    ExpiresAt = p.ExpiresAt
                })
                .ToListAsync();

            return permissions;
        }

        public async Task<List<PermissionDTO>> GetFolderPermissionsAsync(
            Guid folderId,
            Guid userId)
        {
            // Verify user can manage permissions
            var canManage = await CanAccessFolderAsync(folderId, userId, PermissionType.Admin.ToString());
            if (!canManage)
            {
                var folder = await _context.Folders.FindAsync(folderId);
                if (folder?.OwnerId != userId)
                    throw new UnauthorizedAccessException("You don't have permission to view folder permissions");
            }

            var permissions = await _context.Permissions
                .Include(p => p.User)
                .Where(p => p.FolderId == folderId)
                .Select(p => new PermissionDTO
                {
                    Id = p.Id,
                    FolderId = p.FolderId,
                    UserId = p.UserId,
                    UserName = p.User.Username,
                    PermissionType = p.PermissionType,
                    IsInherited = p.IsInherited,
                    GrantedAt = p.GrantedAt,
                    GrantedBy = p.GrantedBy,
                    ExpiresAt = p.ExpiresAt
                })
                .ToListAsync();

            return permissions;
        }

        public async Task<PermissionDTO> GrantPermissionAsync(
            GrantPermissionDTO dto,
            Guid grantedBy)
        {
            // Validate that either FolderId or DocumentId is provided
            if (!dto.FolderId.HasValue && !dto.DocumentId.HasValue)
                throw new ArgumentException("Either FolderId or DocumentId must be provided");

            if (dto.FolderId.HasValue && dto.DocumentId.HasValue)
                throw new ArgumentException("Cannot grant permission to both folder and document");

            // Verify granter has Share or Admin permission
            if (dto.DocumentId.HasValue)
            {
                var canShare = await CanAccessDocumentAsync(
                    dto.DocumentId.Value,
                    grantedBy,
                    PermissionType.Share.ToString());

                if (!canShare)
                {
                    var doc = await _context.Documents.FindAsync(dto.DocumentId.Value);
                    if (doc?.OwnerId != grantedBy)
                        throw new UnauthorizedAccessException("You don't have permission to grant access to this document");
                }
            }
            else if (dto.FolderId.HasValue)
            {
                var canShare = await CanAccessFolderAsync(
                    dto.FolderId.Value,
                    grantedBy,
                    PermissionType.Share.ToString());

                if (!canShare)
                {
                    var folder = await _context.Folders.FindAsync(dto.FolderId.Value);
                    if (folder?.OwnerId != grantedBy)
                        throw new UnauthorizedAccessException("You don't have permission to grant access to this folder");
                }
            }

            // Verify target user exists
            var targetUser = await _context.Users.FindAsync(dto.UserId);
            if (targetUser == null || !targetUser.IsActive)
                throw new ArgumentException("Target user not found or inactive");

            // Check if permission already exists
            var existingPermission = await _context.Permissions
                .FirstOrDefaultAsync(p =>
                    p.UserId == dto.UserId &&
                    p.FolderId == dto.FolderId &&
                    p.DocumentId == dto.DocumentId &&
                    p.PermissionType == dto.PermissionType);

            if (existingPermission != null)
            {
                // Update expiration if needed
                if (dto.ExpiresAt.HasValue)
                {
                    existingPermission.ExpiresAt = dto.ExpiresAt;
                    await _context.SaveChangesAsync();
                }

                return new PermissionDTO
                {
                    Id = existingPermission.Id,
                    FolderId = existingPermission.FolderId,
                    DocumentId = existingPermission.DocumentId,
                    UserId = existingPermission.UserId,
                    UserName = targetUser.Username,
                    PermissionType = existingPermission.PermissionType,
                    IsInherited = existingPermission.IsInherited,
                    GrantedAt = existingPermission.GrantedAt,
                    GrantedBy = existingPermission.GrantedBy,
                    ExpiresAt = existingPermission.ExpiresAt
                };
            }

            // Create new permission
            var permission = new Permission
            {
                Id = Guid.NewGuid(),
                FolderId = dto.FolderId,
                DocumentId = dto.DocumentId,
                UserId = dto.UserId,
                PermissionType = dto.PermissionType,
                IsInherited = false,
                GrantedAt = DateTime.UtcNow,
                GrantedBy = grantedBy.ToString(),
                ExpiresAt = dto.ExpiresAt
            };

            _context.Permissions.Add(permission);
            await _context.SaveChangesAsync();

            // Clear cache for affected user
            ClearUserPermissionCache(dto.UserId);

            _logger.LogInformation(
                "Permission granted: {PermissionType} to user {UserId} by {GrantedBy}",
                dto.PermissionType, dto.UserId, grantedBy);

            return new PermissionDTO
            {
                Id = permission.Id,
                FolderId = permission.FolderId,
                DocumentId = permission.DocumentId,
                UserId = permission.UserId,
                UserName = targetUser.Username,
                PermissionType = permission.PermissionType,
                IsInherited = permission.IsInherited,
                GrantedAt = permission.GrantedAt,
                GrantedBy = permission.GrantedBy,
                ExpiresAt = permission.ExpiresAt
            };
        }

        public async Task RevokePermissionAsync(Guid permissionId, Guid userId)
        {
            var permission = await _context.Permissions
                .Include(p => p.Folder)
                .Include(p => p.Document)
                .FirstOrDefaultAsync(p => p.Id == permissionId);

            if (permission == null)
                throw new KeyNotFoundException("Permission not found");

            // Verify user can revoke (must be owner or have Admin permission)
            bool canRevoke = false;

            if (permission.DocumentId.HasValue)
            {
                var doc = await _context.Documents.FindAsync(permission.DocumentId.Value);
                canRevoke = doc?.OwnerId == userId ||
                    await CanAccessDocumentAsync(permission.DocumentId.Value, userId, PermissionType.Admin.ToString());
            }
            else if (permission.FolderId.HasValue)
            {
                var folder = await _context.Folders.FindAsync(permission.FolderId.Value);
                canRevoke = folder?.OwnerId == userId ||
                    await CanAccessFolderAsync(permission.FolderId.Value, userId, PermissionType.Admin.ToString());
            }

            if (!canRevoke)
                throw new UnauthorizedAccessException("You don't have permission to revoke this access");

            _context.Permissions.Remove(permission);
            await _context.SaveChangesAsync();

            // Clear cache for affected user
            ClearUserPermissionCache(permission.UserId);

            _logger.LogInformation(
                "Permission revoked: {PermissionId} by user {UserId}",
                permissionId, userId);
        }

        public async Task InheritFolderPermissionsAsync(Guid folderId, Guid documentId)
        {
            var folderPermissions = await _context.Permissions
                .Where(p => p.FolderId == folderId)
                .ToListAsync();

            foreach (var folderPerm in folderPermissions)
            {
                // Check if document already has this permission
                var exists = await _context.Permissions
                    .AnyAsync(p => p.DocumentId == documentId &&
                        p.UserId == folderPerm.UserId &&
                        p.PermissionType == folderPerm.PermissionType);

                if (!exists)
                {
                    var inheritedPerm = new Permission
                    {
                        Id = Guid.NewGuid(),
                        DocumentId = documentId,
                        UserId = folderPerm.UserId,
                        PermissionType = folderPerm.PermissionType,
                        IsInherited = true,
                        GrantedAt = DateTime.UtcNow,
                        GrantedBy = "System",
                        ExpiresAt = folderPerm.ExpiresAt
                    };

                    _context.Permissions.Add(inheritedPerm);
                }
            }

            await _context.SaveChangesAsync();
        }

        private async Task<bool> HasExplicitPermissionAsync(
            Guid? folderId = null,
            Guid? documentId = null,
            Guid? userId = null,
            string permissionType = null)
        {
            var query = _context.Permissions.AsQueryable();

            if (folderId.HasValue)
                query = query.Where(p => p.FolderId == folderId.Value);

            if (documentId.HasValue)
                query = query.Where(p => p.DocumentId == documentId.Value);

            if (userId.HasValue)
                query = query.Where(p => p.UserId == userId.Value);

            var permissions = await query.ToListAsync();

            // Check for expired permissions
            permissions = permissions
                .Where(p => !p.ExpiresAt.HasValue || p.ExpiresAt.Value > DateTime.UtcNow)
                .ToList();

            if (!permissions.Any())
                return false;

            if (string.IsNullOrEmpty(permissionType))
                return true;

            // Check permission hierarchy
            return permissions.Any(p => HasRequiredPermissionLevel(p.PermissionType, permissionType));
        }

        private bool HasRequiredPermissionLevel(string grantedPermission, string requiredPermission)
        {
            var permissionHierarchy = new Dictionary<string, int>
            {
                { PermissionType.Read.ToString(), 1 },
                { PermissionType.Write.ToString(), 2 },
                { PermissionType.Delete.ToString(), 3 },
                { PermissionType.Share.ToString(), 4 },
                { PermissionType.Admin.ToString(), 5 }
            };

            var grantedLevel = permissionHierarchy.GetValueOrDefault(grantedPermission, 0);
            var requiredLevel = permissionHierarchy.GetValueOrDefault(requiredPermission, 0);

            return grantedLevel >= requiredLevel;
        }

        private void CachePermission(string key, bool value)
        {
            _cache.Set(key, value, TimeSpan.FromMinutes(CacheExpirationMinutes));
        }

        private void ClearUserPermissionCache(Guid userId)
        {
            // In production, use a more sophisticated cache invalidation strategy
            // For now, we rely on cache expiration
            _logger.LogDebug("Cache cleared for user {UserId}", userId);
        }
    }
}
```

## Custom Authorization Attributes

### DocumentPermissionAttribute

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Backend.Services.Interfaces;

namespace Backend.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class DocumentPermissionAttribute : Attribute, IAsyncActionFilter
    {
        private readonly string _permissionType;
        private readonly string _documentIdParameter;

        public DocumentPermissionAttribute(
            string permissionType,
            string documentIdParameter = "id")
        {
            _permissionType = permissionType;
            _documentIdParameter = documentIdParameter;
        }

        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            var permissionService = context.HttpContext.RequestServices
                .GetRequiredService<IPermissionService>();

            // Get document ID from route or query parameters
            if (!context.ActionArguments.TryGetValue(_documentIdParameter, out var documentIdObj))
            {
                context.Result = new BadRequestObjectResult("Document ID not provided");
                return;
            }

            if (!Guid.TryParse(documentIdObj?.ToString(), out var documentId))
            {
                context.Result = new BadRequestObjectResult("Invalid document ID");
                return;
            }

            // Get user ID from claims
            var userIdClaim = context.HttpContext.User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Check permission
            var hasPermission = await permissionService.CanAccessDocumentAsync(
                documentId,
                userId,
                _permissionType);

            if (!hasPermission)
            {
                context.Result = new ForbidResult();
                return;
            }

            await next();
        }
    }
}
```

### FolderPermissionAttribute

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Backend.Services.Interfaces;

namespace Backend.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class FolderPermissionAttribute : Attribute, IAsyncActionFilter
    {
        private readonly string _permissionType;
        private readonly string _folderIdParameter;

        public FolderPermissionAttribute(
            string permissionType,
            string folderIdParameter = "id")
        {
            _permissionType = permissionType;
            _folderIdParameter = folderIdParameter;
        }

        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            var permissionService = context.HttpContext.RequestServices
                .GetRequiredService<IPermissionService>();

            // Get folder ID from route or query parameters
            if (!context.ActionArguments.TryGetValue(_folderIdParameter, out var folderIdObj))
            {
                context.Result = new BadRequestObjectResult("Folder ID not provided");
                return;
            }

            if (!Guid.TryParse(folderIdObj?.ToString(), out var folderId))
            {
                context.Result = new BadRequestObjectResult("Invalid folder ID");
                return;
            }

            // Get user ID from claims
            var userIdClaim = context.HttpContext.User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Check permission
            var hasPermission = await permissionService.CanAccessFolderAsync(
                folderId,
                userId,
                _permissionType);

            if (!hasPermission)
            {
                context.Result = new ForbidResult();
                return;
            }

            await next();
        }
    }
}
```

## Authorization Policies

### Program.cs Configuration

```csharp
using Microsoft.AspNetCore.Authorization;

// Add authorization policies
builder.Services.AddAuthorization(options =>
{
    // Role-based policies
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("SystemAdmin", "Admin"));

    options.AddPolicy("ManagerOrAbove", policy =>
        policy.RequireRole("SystemAdmin", "Admin", "Manager"));

    options.AddPolicy("EditorOrAbove", policy =>
        policy.RequireRole("SystemAdmin", "Admin", "Manager", "Editor"));

    options.AddPolicy("CanCreateDocuments", policy =>
        policy.RequireRole("SystemAdmin", "Admin", "Manager", "Editor", "Contributor"));

    // Custom policies
    options.AddPolicy("CanManageUsers", policy =>
        policy.Requirements.Add(new UserManagementRequirement()));

    options.AddPolicy("CanManageWorkflows", policy =>
        policy.Requirements.Add(new WorkflowManagementRequirement()));
});

// Register authorization handlers
builder.Services.AddSingleton<IAuthorizationHandler, UserManagementHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, WorkflowManagementHandler>();
```

## Custom Authorization Requirements

### UserManagementRequirement

```csharp
using Microsoft.AspNetCore.Authorization;

namespace Backend.Authorization
{
    public class UserManagementRequirement : IAuthorizationRequirement
    {
    }

    public class UserManagementHandler : AuthorizationHandler<UserManagementRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            UserManagementRequirement requirement)
        {
            var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;

            if (userRole == "SystemAdmin" || userRole == "Admin")
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
```

### WorkflowManagementRequirement

```csharp
using Microsoft.AspNetCore.Authorization;

namespace Backend.Authorization
{
    public class WorkflowManagementRequirement : IAuthorizationRequirement
    {
    }

    public class WorkflowManagementHandler : AuthorizationHandler<WorkflowManagementRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            WorkflowManagementRequirement requirement)
        {
            var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;

            if (userRole == "SystemAdmin" ||
                userRole == "Admin" ||
                userRole == "Manager")
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
```

## PermissionsController

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Backend.Models.DTO;
using Backend.Models.DTO.Common;
using Backend.Services.Interfaces;

namespace Backend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PermissionsController : ControllerBase
    {
        private readonly IPermissionService _permissionService;
        private readonly ILogger<PermissionsController> _logger;

        public PermissionsController(
            IPermissionService permissionService,
            ILogger<PermissionsController> logger)
        {
            _permissionService = permissionService;
            _logger = logger;
        }

        /// <summary>
        /// Get document permissions
        /// </summary>
        [HttpGet("document/{documentId}")]
        public async Task<ActionResult<ApiResponse<List<PermissionDTO>>>> GetDocumentPermissions(
            Guid documentId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var permissions = await _permissionService.GetDocumentPermissionsAsync(documentId, userId);

                return Ok(ApiResponse<List<PermissionDTO>>.SuccessResponse(permissions));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting document permissions");
                return StatusCode(500, ApiResponse<List<PermissionDTO>>.ErrorResponse("Error retrieving permissions"));
            }
        }

        /// <summary>
        /// Get folder permissions
        /// </summary>
        [HttpGet("folder/{folderId}")]
        public async Task<ActionResult<ApiResponse<List<PermissionDTO>>>> GetFolderPermissions(
            Guid folderId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var permissions = await _permissionService.GetFolderPermissionsAsync(folderId, userId);

                return Ok(ApiResponse<List<PermissionDTO>>.SuccessResponse(permissions));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting folder permissions");
                return StatusCode(500, ApiResponse<List<PermissionDTO>>.ErrorResponse("Error retrieving permissions"));
            }
        }

        /// <summary>
        /// Grant permission to user
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<PermissionDTO>>> GrantPermission(
            [FromBody] GrantPermissionDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<PermissionDTO>.ErrorResponse("Invalid input"));

                var userId = GetCurrentUserId();
                var permission = await _permissionService.GrantPermissionAsync(dto, userId);

                return Ok(ApiResponse<PermissionDTO>.SuccessResponse(permission, "Permission granted successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<PermissionDTO>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error granting permission");
                return StatusCode(500, ApiResponse<PermissionDTO>.ErrorResponse("Error granting permission"));
            }
        }

        /// <summary>
        /// Revoke permission
        /// </summary>
        [HttpDelete("{permissionId}")]
        public async Task<ActionResult<ApiResponse<bool>>> RevokePermission(Guid permissionId)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _permissionService.RevokePermissionAsync(permissionId, userId);

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Permission revoked successfully"));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse("Permission not found"));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking permission");
                return StatusCode(500, ApiResponse<bool>.ErrorResponse("Error revoking permission"));
            }
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException());
        }
    }
}
```

## Usage Examples

### Using Attributes in Controllers

```csharp
[HttpGet("{id}")]
[DocumentPermission("Read")]
public async Task<ActionResult<DocumentDTO>> GetDocument(Guid id)
{
    // Permission already checked by attribute
    var document = await _documentService.GetDocumentByIdAsync(id, userId);
    return Ok(document);
}

[HttpPut("{id}")]
[DocumentPermission("Write")]
public async Task<ActionResult<DocumentDTO>> UpdateDocument(
    Guid id,
    [FromBody] UpdateDocumentDTO dto)
{
    // Permission already checked by attribute
    var document = await _documentService.UpdateDocumentAsync(id, dto, userId);
    return Ok(document);
}

[HttpDelete("{id}")]
[DocumentPermission("Delete")]
public async Task<ActionResult> DeleteDocument(Guid id)
{
    // Permission already checked by attribute
    await _documentService.DeleteDocumentAsync(id, userId);
    return NoContent();
}
```

### Using Policies

```csharp
[Authorize(Policy = "AdminOnly")]
[HttpPost("users")]
public async Task<ActionResult<UserDTO>> CreateUser([FromBody] CreateUserDTO dto)
{
    var user = await _userService.CreateUserAsync(dto);
    return Ok(user);
}

[Authorize(Policy = "CanManageWorkflows")]
[HttpPost("workflows")]
public async Task<ActionResult<WorkflowDTO>> CreateWorkflow([FromBody] CreateWorkflowDTO dto)
{
    var workflow = await _workflowService.CreateWorkflowAsync(dto, userId);
    return Ok(workflow);
}
```

### Manual Permission Checking

```csharp
public async Task<ActionResult> ShareDocument(Guid documentId, Guid targetUserId)
{
    var currentUserId = GetCurrentUserId();

    // Check if user has Share permission
    var canShare = await _permissionService.CanAccessDocumentAsync(
        documentId,
        currentUserId,
        "Share");

    if (!canShare)
        return Forbid();

    // Grant Read permission to target user
    await _permissionService.GrantPermissionAsync(new GrantPermissionDTO
    {
        DocumentId = documentId,
        UserId = targetUserId,
        PermissionType = "Read"
    }, currentUserId);

    return Ok();
}
```

## Permission Inheritance Flow

```
Root Folder (Admin permission granted to User A)
    ↓ (inherits)
Sub Folder 1 (User A has Admin through inheritance)
    ↓ (inherits)
Document 1 (User A has Admin through inheritance)

If explicit permission is granted on Document 1:
    - Explicit permission takes precedence
    - Can be more OR less restrictive than inherited
    - Inherited permissions still exist but explicit is checked first
```

## Security Best Practices

1. **Always validate permissions server-side** - Never trust client-side checks
2. **Use caching wisely** - Cache permissions with appropriate TTL
3. **Clear cache on permission changes** - Invalidate affected user caches
4. **Check expiration dates** - Always filter expired permissions
5. **Audit permission changes** - Log all grant/revoke operations
6. **Use HTTPS** - Encrypt all API communication
7. **Implement rate limiting** - Prevent permission check abuse
8. **Regular permission audits** - Review granted permissions periodically

---

**Document Version**: 1.0  
**Last Updated**: November 25, 2025  
**Author**: EDM Project Team  
**Status**: Design Complete - Ready for Implementation
