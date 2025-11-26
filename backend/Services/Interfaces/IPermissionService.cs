using Backend.Models.DTO.Permissions;

namespace Backend.Services.Interfaces;

public interface IPermissionService
{
    Task<bool> CanAccessFolderAsync(Guid userId, Guid folderId, string requiredPermission);
    Task<bool> CanAccessDocumentAsync(Guid userId, Guid documentId, string requiredPermission);
    Task<PermissionDTO?> GrantPermissionAsync(CreatePermissionRequest request, string grantedBy);
    Task<bool> RevokePermissionAsync(Guid permissionId);
    Task<List<PermissionDTO>> GetUserPermissionsAsync(Guid userId);
    Task<List<PermissionDTO>> GetFolderPermissionsAsync(Guid folderId);
    Task<List<PermissionDTO>> GetDocumentPermissionsAsync(Guid documentId);
}
