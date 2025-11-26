namespace Backend.Models.EDM
{
    public class Document : SoftDeletableEntity
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid FolderId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public long FileSizeBytes { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string MimeType { get; set; } = string.Empty;
        public string FileExtension { get; set; } = string.Empty;
        public int CurrentVersion { get; set; } = 1;
        public string Status { get; set; } = "Active";
        public Guid OwnerId { get; set; }
        public bool IsPublic { get; set; } = false;
        public string? CustomMetadata { get; set; }
        public int ViewCount { get; set; } = 0;
        public int DownloadCount { get; set; } = 0;
        public DateTime? ExpirationDate { get; set; }
        
        // Navigation properties
        public User Owner { get; set; } = null!;
        public Folder Folder { get; set; } = null!;
        public ICollection<DocumentVersion> Versions { get; set; } = new List<DocumentVersion>();
        public ICollection<DocumentTag> DocumentTags { get; set; } = new List<DocumentTag>();
        public ICollection<Permission> Permissions { get; set; } = new List<Permission>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Workflow> Workflows { get; set; } = new List<Workflow>();
    }
}
