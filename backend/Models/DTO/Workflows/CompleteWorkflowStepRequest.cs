using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTO.Workflows;

public class CompleteWorkflowStepRequest
{
    [Required]
    public string Status { get; set; } = string.Empty; // Approved, Rejected

    [StringLength(1000)]
    public string? Comment { get; set; }
}
