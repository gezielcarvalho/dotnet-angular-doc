namespace Backend.Models.Document
{
    public class DocumentVersion : BaseEntity
    {
        public Guid DocumentId { get; set; }
        public int VersionNumber { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public long FileSizeBytes { get; set; }
        public string? ChangeComment { get; set; }
        public string MimeType { get; set; } = string.Empty;
        public bool IsCurrentVersion { get; set; } = false;
        
        // Navigation properties
        public Document Document { get; set; } = null!;
    }
}
