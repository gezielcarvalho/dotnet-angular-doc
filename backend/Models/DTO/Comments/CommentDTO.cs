namespace Backend.Models.DTO.Comments;

public class CommentDTO
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public Guid? ParentCommentId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public List<CommentDTO> Replies { get; set; } = new();
}
