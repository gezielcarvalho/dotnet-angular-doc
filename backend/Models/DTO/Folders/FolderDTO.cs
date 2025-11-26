namespace Backend.Models.DTO.Folders;

public class FolderDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Path { get; set; } = string.Empty;
    public int Level { get; set; }
    public Guid? ParentFolderId { get; set; }
    public string? ParentFolderName { get; set; }
    public bool IsSystemFolder { get; set; }
    public Guid OwnerId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int SubFolderCount { get; set; }
    public int DocumentCount { get; set; }
}
