using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTO.Documents;

public class UpdateDocumentRequest
{
    [Required]
    [StringLength(255)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    public string? Status { get; set; }

    public List<Guid>? TagIds { get; set; }
}
