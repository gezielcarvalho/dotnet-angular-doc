namespace Backend.Models.EDM
{
    public class DocumentTag
    {
        public Guid DocumentId { get; set; }
        public Guid TagId { get; set; }
        
        // Navigation properties
        public Document Document { get; set; } = null!;
        public Tag Tag { get; set; } = null!;
    }
}
