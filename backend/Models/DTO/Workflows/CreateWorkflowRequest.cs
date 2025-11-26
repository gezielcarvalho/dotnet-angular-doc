using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTO.Workflows;

public class CreateWorkflowRequest
{
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    public Guid DocumentId { get; set; }

    [Required]
    public string WorkflowType { get; set; } = string.Empty;

    public DateTime? DueDate { get; set; }

    [Required]
    public List<CreateWorkflowStepRequest> Steps { get; set; } = new();
}

public class CreateWorkflowStepRequest
{
    [Required]
    public string StepName { get; set; } = string.Empty;

    [Required]
    public Guid AssignedToUserId { get; set; }

    public DateTime? DueDate { get; set; }
}
