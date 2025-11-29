namespace Backend.Models.Document
{
    public class Workflow : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid DocumentId { get; set; }
        public string WorkflowType { get; set; } = "Approval";
        public string Status { get; set; } = "Pending";
        public int CurrentStepOrder { get; set; } = 1;
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? CompletedBy { get; set; }
        
        // Navigation properties
        public Document Document { get; set; } = null!;
        public ICollection<WorkflowStep> Steps { get; set; } = new List<WorkflowStep>();
    }
}
