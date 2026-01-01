using Backend.Models.Document;
using FluentAssertions;

namespace backend.tests.Models;

public class WorkflowStepTests
{
    [Fact]
    public void WorkflowStep_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var workflowStep = new WorkflowStep();

        // Assert - BaseEntity properties
        workflowStep.Id.Should().BeEmpty();
        workflowStep.CreatedAt.Should().Be(default(DateTime));
        workflowStep.CreatedBy.Should().BeEmpty();
        workflowStep.ModifiedAt.Should().BeNull();
        workflowStep.ModifiedBy.Should().BeNull();

        // Assert - WorkflowStep properties
        workflowStep.WorkflowId.Should().BeEmpty();
        workflowStep.StepOrder.Should().Be(0);
        workflowStep.StepName.Should().BeEmpty();
        workflowStep.AssignedToUserId.Should().BeEmpty();
        workflowStep.Status.Should().Be("Pending");
        workflowStep.Comment.Should().BeNull();
        workflowStep.DueDate.Should().BeNull();
        workflowStep.CompletedAt.Should().BeNull();
        workflowStep.CompletedBy.Should().BeNull();

        // Assert - Navigation properties
        workflowStep.Workflow.Should().BeNull();
        workflowStep.AssignedToUser.Should().BeNull();
    }

    [Fact]
    public void WorkflowStep_ShouldSetProperties()
    {
        // Arrange
        var workflowId = Guid.NewGuid();
        var assignedToUserId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        var modifiedAt = DateTime.UtcNow.AddHours(1);
        var dueDate = DateTime.UtcNow.AddDays(5);
        var completedAt = DateTime.UtcNow.AddDays(3);

        // Act
        var workflowStep = new WorkflowStep
        {
            Id = Guid.NewGuid(),
            WorkflowId = workflowId,
            StepOrder = 2,
            StepName = "Technical Review",
            AssignedToUserId = assignedToUserId,
            Status = "InProgress",
            Comment = "Please review the technical specifications",
            DueDate = dueDate,
            CompletedAt = completedAt,
            CompletedBy = "reviewer@example.com",
            CreatedAt = createdAt,
            CreatedBy = "admin@example.com",
            ModifiedAt = modifiedAt,
            ModifiedBy = "system@example.com"
        };

        // Assert - BaseEntity properties
        workflowStep.Id.Should().NotBeEmpty();
        workflowStep.CreatedAt.Should().Be(createdAt);
        workflowStep.CreatedBy.Should().Be("admin@example.com");
        workflowStep.ModifiedAt.Should().Be(modifiedAt);
        workflowStep.ModifiedBy.Should().Be("system@example.com");

        // Assert - WorkflowStep properties
        workflowStep.WorkflowId.Should().Be(workflowId);
        workflowStep.StepOrder.Should().Be(2);
        workflowStep.StepName.Should().Be("Technical Review");
        workflowStep.AssignedToUserId.Should().Be(assignedToUserId);
        workflowStep.Status.Should().Be("InProgress");
        workflowStep.Comment.Should().Be("Please review the technical specifications");
        workflowStep.DueDate.Should().Be(dueDate);
        workflowStep.CompletedAt.Should().Be(completedAt);
        workflowStep.CompletedBy.Should().Be("reviewer@example.com");
    }

    [Fact]
    public void WorkflowStep_ShouldHandleNullOptionalProperties()
    {
        // Arrange & Act
        var workflowStep = new WorkflowStep
        {
            WorkflowId = Guid.NewGuid(),
            StepName = "Test Step",
            AssignedToUserId = Guid.NewGuid(),
            Comment = null,
            DueDate = null,
            CompletedAt = null,
            CompletedBy = null,
            ModifiedAt = null,
            ModifiedBy = null
        };

        // Assert
        workflowStep.Comment.Should().BeNull();
        workflowStep.DueDate.Should().BeNull();
        workflowStep.CompletedAt.Should().BeNull();
        workflowStep.CompletedBy.Should().BeNull();
        workflowStep.ModifiedAt.Should().BeNull();
        workflowStep.ModifiedBy.Should().BeNull();
    }

    [Fact]
    public void WorkflowStep_ShouldDefaultStatusToPending()
    {
        // Arrange & Act
        var workflowStep = new WorkflowStep();

        // Assert
        workflowStep.Status.Should().Be("Pending");
    }

    [Fact]
    public void WorkflowStep_ShouldAllowStatusChanges()
    {
        // Arrange
        var workflowStep = new WorkflowStep();

        // Act - Change status through different workflow states
        workflowStep.Status = "InProgress";
        workflowStep.Status.Should().Be("InProgress");

        workflowStep.Status = "Completed";
        workflowStep.Status.Should().Be("Completed");

        workflowStep.Status = "Rejected";
        workflowStep.Status.Should().Be("Rejected");
    }

    [Fact]
    public void WorkflowStep_ShouldHandleCompletionData()
    {
        // Arrange
        var workflowStep = new WorkflowStep
        {
            Status = "Pending"
        };

        // Act - Mark as completed
        workflowStep.Status = "Completed";
        workflowStep.CompletedAt = DateTime.UtcNow;
        workflowStep.CompletedBy = "approver@example.com";
        workflowStep.Comment = "Approved with minor changes requested";

        // Assert
        workflowStep.Status.Should().Be("Completed");
        workflowStep.CompletedAt.Should().NotBeNull();
        workflowStep.CompletedBy.Should().Be("approver@example.com");
        workflowStep.Comment.Should().Be("Approved with minor changes requested");
    }

    [Fact]
    public void WorkflowStep_ShouldSupportStepOrdering()
    {
        // Arrange
        var step1 = new WorkflowStep { StepOrder = 1, StepName = "Initial Review" };
        var step2 = new WorkflowStep { StepOrder = 2, StepName = "Technical Review" };
        var step3 = new WorkflowStep { StepOrder = 3, StepName = "Final Approval" };

        // Assert
        step1.StepOrder.Should().Be(1);
        step2.StepOrder.Should().Be(2);
        step3.StepOrder.Should().Be(3);

        step1.StepName.Should().Be("Initial Review");
        step2.StepName.Should().Be("Technical Review");
        step3.StepName.Should().Be("Final Approval");
    }
}