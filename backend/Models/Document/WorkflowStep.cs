namespace Backend.Models.Document
{
    public class WorkflowStep : BaseEntity
    {
        public Guid WorkflowId { get; set; }
        public int StepOrder { get; set; }
        public string StepName { get; set; } = string.Empty;
        public Guid AssignedToUserId { get; set; }
        public string Status { get; set; } = "Pending";
        public string? Comment { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? CompletedBy { get; set; }
        
        // Navigation properties
        public Workflow Workflow { get; set; } = null!;
        public User AssignedToUser { get; set; } = null!;
    }
}
