using Backend.Models.DTO.Workflows;
using FluentAssertions;

namespace backend.tests.Models.DTO.Workflows;

public class WorkflowStepDTOTests
{
    [Fact]
    public void WorkflowStepDTO_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var dto = new WorkflowStepDTO();

        // Assert
        dto.Id.Should().BeEmpty();
        dto.StepOrder.Should().Be(0);
        dto.StepName.Should().BeEmpty();
        dto.AssignedToUserId.Should().BeEmpty();
        dto.AssignedToUserName.Should().BeEmpty();
        dto.Status.Should().BeEmpty();
        dto.Comment.Should().BeNull();
        dto.DueDate.Should().BeNull();
        dto.CompletedAt.Should().BeNull();
        dto.CompletedBy.Should().BeNull();
    }

    [Fact]
    public void WorkflowStepDTO_ShouldSetProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var assignedToUserId = Guid.NewGuid();
        var dueDate = DateTime.UtcNow;
        var completedAt = DateTime.UtcNow.AddDays(1);

        // Act
        var dto = new WorkflowStepDTO
        {
            Id = id,
            StepOrder = 1,
            StepName = "Review Document",
            AssignedToUserId = assignedToUserId,
            AssignedToUserName = "John Doe",
            Status = "Pending",
            Comment = "Please review carefully",
            DueDate = dueDate,
            CompletedAt = completedAt,
            CompletedBy = "Jane Smith"
        };

        // Assert
        dto.Id.Should().Be(id);
        dto.StepOrder.Should().Be(1);
        dto.StepName.Should().Be("Review Document");
        dto.AssignedToUserId.Should().Be(assignedToUserId);
        dto.AssignedToUserName.Should().Be("John Doe");
        dto.Status.Should().Be("Pending");
        dto.Comment.Should().Be("Please review carefully");
        dto.DueDate.Should().Be(dueDate);
        dto.CompletedAt.Should().Be(completedAt);
        dto.CompletedBy.Should().Be("Jane Smith");
    }
}