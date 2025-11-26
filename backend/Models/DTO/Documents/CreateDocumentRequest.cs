using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTO.Documents;

public class CreateDocumentRequest
{
    [Required]
    [StringLength(255)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    public Guid FolderId { get; set; }

    public List<Guid>? TagIds { get; set; }
}
