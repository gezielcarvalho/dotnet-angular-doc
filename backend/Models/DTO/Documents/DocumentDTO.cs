namespace Backend.Models.DTO.Documents;

public class DocumentDTO
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string FileSizeFormatted => FormatFileSize(FileSizeBytes);
    public string Status { get; set; } = string.Empty;
    public int CurrentVersion { get; set; }
    public Guid FolderId { get; set; }
    public string FolderPath { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public List<string> Tags { get; set; } = new();
    public bool HasActiveWorkflow { get; set; }

    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}
