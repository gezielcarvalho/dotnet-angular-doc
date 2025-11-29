namespace Backend.Models.Document
{
    public class Tag : SoftDeletableEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Color { get; set; } = "#0066CC";
        
        // Navigation properties
        public ICollection<DocumentTag> DocumentTags { get; set; } = new List<DocumentTag>();
    }
}
