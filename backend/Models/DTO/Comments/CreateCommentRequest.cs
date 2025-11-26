using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTO.Comments;

public class CreateCommentRequest
{
    [Required]
    [StringLength(2000)]
    public string Content { get; set; } = string.Empty;

    public Guid? ParentCommentId { get; set; }
}
