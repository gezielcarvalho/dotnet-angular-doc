using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTO.Permissions;

public class CreatePermissionRequest
{
    [Required]
    public Guid UserId { get; set; }

    public Guid? FolderId { get; set; }

    public Guid? DocumentId { get; set; }

    [Required]
    public string PermissionType { get; set; } = string.Empty;
}
