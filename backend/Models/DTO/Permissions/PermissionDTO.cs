namespace Backend.Models.DTO.Permissions;

public class PermissionDTO
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public Guid? FolderId { get; set; }
    public string? FolderPath { get; set; }
    public Guid? DocumentId { get; set; }
    public string? DocumentTitle { get; set; }
    public string PermissionType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
