namespace Backend.Models.DTO.Workflows;

public class WorkflowStepDTO
{
    public Guid Id { get; set; }
    public int StepOrder { get; set; }
    public string StepName { get; set; } = string.Empty;
    public Guid AssignedToUserId { get; set; }
    public string AssignedToUserName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? CompletedBy { get; set; }
}
