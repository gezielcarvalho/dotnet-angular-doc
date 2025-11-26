namespace Backend.Models.EDM
{
    public class Permission : BaseEntity
    {
        public Guid? FolderId { get; set; }
        public Guid? DocumentId { get; set; }
        public Guid UserId { get; set; }
        public string PermissionType { get; set; } = "Read";
        public bool IsInherited { get; set; } = false;
        public DateTime GrantedAt { get; set; }
        public string GrantedBy { get; set; } = string.Empty;
        public DateTime? ExpiresAt { get; set; }
        
        // Navigation properties
        public Folder? Folder { get; set; }
        public Document? Document { get; set; }
        public User User { get; set; } = null!;
    }
}
