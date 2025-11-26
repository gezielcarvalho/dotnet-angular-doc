namespace Backend.Models.DTO.Workflows;

public class WorkflowDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid DocumentId { get; set; }
    public string DocumentTitle { get; set; } = string.Empty;
    public string WorkflowType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int CurrentStepOrder { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<WorkflowStepDTO> Steps { get; set; } = new();
}
