namespace Backend.Models.EDM
{
    public class DocumentVersion : BaseEntity
    {
        public Guid DocumentId { get; set; }
        public int VersionNumber { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string MimeType { get; set; } = string.Empty;
        public string? VersionComment { get; set; }
        public bool IsCurrentVersion { get; set; } = false;
        
        // Navigation properties
        public Document Document { get; set; } = null!;
    }
}
