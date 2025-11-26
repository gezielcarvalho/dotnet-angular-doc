using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTO.Tags;

public class CreateTagRequest
{
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Description { get; set; }

    [RegularExpression(@"^#([A-Fa-f0-9]{6})$", ErrorMessage = "Color must be a valid hex color code (e.g., #FF0000)")]
    public string Color { get; set; } = "#000000";
}
