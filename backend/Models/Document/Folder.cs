namespace Backend.Models.Document
{
    public class Folder : SoftDeletableEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? ParentFolderId { get; set; }
        public string Path { get; set; } = "/";
        public int Level { get; set; } = 0;
        public bool IsSystemFolder { get; set; } = false;
        public Guid OwnerId { get; set; }
        
        // Navigation properties
        public User Owner { get; set; } = null!;
        public Folder? ParentFolder { get; set; }
        public ICollection<Folder> SubFolders { get; set; } = new List<Folder>();
        public ICollection<Document> Documents { get; set; } = new List<Document>();
        public ICollection<Permission> Permissions { get; set; } = new List<Permission>();
    }
}
