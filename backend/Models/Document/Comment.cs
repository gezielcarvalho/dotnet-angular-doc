namespace Backend.Models.Document
{
    public class Comment : SoftDeletableEntity
    {
        public Guid DocumentId { get; set; }
        public Guid? ParentCommentId { get; set; }
        public Guid UserId { get; set; }
        public string Text { get; set; } = string.Empty;
        public bool IsResolved { get; set; } = false;
        
        // Navigation properties
        public Document Document { get; set; } = null!;
        public User User { get; set; } = null!;
        public Comment? ParentComment { get; set; }
        public ICollection<Comment> Replies { get; set; } = new List<Comment>();
    }
}
