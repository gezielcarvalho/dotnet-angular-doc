using Backend.Models.DTO.Common;
using Backend.Models.DTO.Permissions;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PermissionsController : ControllerBase
{
    private readonly IPermissionService _permissionService;

    public PermissionsController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<ApiResponse<List<PermissionDTO>>>> GetUserPermissions(Guid userId)
    {
        var currentUserId = GetCurrentUserId();
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        // Only admins or the user themselves can view permissions
        if (currentUserId != userId && userRole != "SystemAdmin" && userRole != "Admin")
            return Forbid();

        var permissions = await _permissionService.GetUserPermissionsAsync(userId);
        return Ok(ApiResponse<List<PermissionDTO>>.SuccessResponse(permissions));
    }

    [HttpGet("folder/{folderId}")]
    public async Task<ActionResult<ApiResponse<List<PermissionDTO>>>> GetFolderPermissions(Guid folderId)
    {
        var userId = GetCurrentUserId();

        if (!await _permissionService.CanAccessFolderAsync(userId, folderId, "Admin"))
            return Forbid();

        var permissions = await _permissionService.GetFolderPermissionsAsync(folderId);
        return Ok(ApiResponse<List<PermissionDTO>>.SuccessResponse(permissions));
    }

    [HttpGet("document/{documentId}")]
    public async Task<ActionResult<ApiResponse<List<PermissionDTO>>>> GetDocumentPermissions(Guid documentId)
    {
        var userId = GetCurrentUserId();

        if (!await _permissionService.CanAccessDocumentAsync(userId, documentId, "Admin"))
            return Forbid();

        var permissions = await _permissionService.GetDocumentPermissionsAsync(documentId);
        return Ok(ApiResponse<List<PermissionDTO>>.SuccessResponse(permissions));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PermissionDTO>>> GrantPermission([FromBody] CreatePermissionRequest request)
    {
        var userId = GetCurrentUserId();
        var username = GetCurrentUsername();

        // Check if user has admin permission on the resource
        if (request.FolderId.HasValue)
        {
            if (!await _permissionService.CanAccessFolderAsync(userId, request.FolderId.Value, "Admin"))
                return Forbid();
        }
        else if (request.DocumentId.HasValue)
        {
            if (!await _permissionService.CanAccessDocumentAsync(userId, request.DocumentId.Value, "Admin"))
                return Forbid();
        }
        else
        {
            return BadRequest(ApiResponse<PermissionDTO>.ErrorResponse("Either FolderId or DocumentId must be provided"));
        }

        var permission = await _permissionService.GrantPermissionAsync(request, username);
        if (permission == null)
            return BadRequest(ApiResponse<PermissionDTO>.ErrorResponse("Failed to grant permission"));

        return Ok(ApiResponse<PermissionDTO>.SuccessResponse(permission, "Permission granted successfully"));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> RevokePermission(Guid id)
    {
        // TODO: Add permission check to ensure user has admin access to the resource
        var success = await _permissionService.RevokePermissionAsync(id);
        
        if (!success)
            return NotFound(ApiResponse<bool>.ErrorResponse("Permission not found"));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Permission revoked successfully"));
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

