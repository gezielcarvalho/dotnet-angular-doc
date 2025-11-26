using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTO.Folders;

public class UpdateFolderRequest
{
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }
}
