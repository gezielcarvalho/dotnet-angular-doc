using Backend.Models.DTO.Workflows;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;

namespace backend.tests.Models.DTO.Workflows;

public class CreateWorkflowRequestTests
{
    [Fact]
    public void CreateWorkflowRequest_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var request = new CreateWorkflowRequest();

        // Assert
        request.Name.Should().BeEmpty();
        request.Description.Should().BeNull();
        request.DocumentId.Should().BeEmpty();
        request.WorkflowType.Should().BeEmpty();
        request.DueDate.Should().BeNull();
        request.Steps.Should().NotBeNull();
        request.Steps.Should().BeEmpty();
    }

    [Fact]
    public void CreateWorkflowRequest_ShouldSetProperties()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var dueDate = DateTime.UtcNow.AddDays(7);
        var steps = new List<CreateWorkflowStepRequest>
        {
            new CreateWorkflowStepRequest
            {
                StepName = "Review Document",
                AssignedToUserId = Guid.NewGuid(),
                DueDate = DateTime.UtcNow.AddDays(1)
            }
        };

        // Act
        var request = new CreateWorkflowRequest
        {
            Name = "Document Approval Workflow",
            Description = "Workflow for document approval process",
            DocumentId = documentId,
            WorkflowType = "Approval",
            DueDate = dueDate,
            Steps = steps
        };

        // Assert
        request.Name.Should().Be("Document Approval Workflow");
        request.Description.Should().Be("Workflow for document approval process");
        request.DocumentId.Should().Be(documentId);
        request.WorkflowType.Should().Be("Approval");
        request.DueDate.Should().Be(dueDate);
        request.Steps.Should().BeEquivalentTo(steps);
    }

    [Fact]
    public void CreateWorkflowRequest_ShouldValidateRequiredFields()
    {
        // Arrange
        var request = new CreateWorkflowRequest();

        // Act
        var validationResults = ValidateModel(request);

        // Assert - Name and WorkflowType are required (string properties with Required attribute)
        // DocumentId is a Guid, which has a default value of Guid.Empty, so validation might not trigger
        validationResults.Should().HaveCountGreaterThan(0);
        var errorMessages = validationResults.Select(v => v.ErrorMessage).ToList();
        errorMessages.Should().ContainMatch("*required*");
    }

    [Fact]
    public void CreateWorkflowRequest_ShouldValidateNameLength()
    {
        // Arrange
        var request = new CreateWorkflowRequest
        {
            Name = new string('A', 256), // Exceeds max length of 255
            DocumentId = Guid.NewGuid(),
            WorkflowType = "Test"
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        validationResults.Should().Contain(v => v.ErrorMessage!.Contains("maximum length"));
    }

    [Fact]
    public void CreateWorkflowRequest_ShouldValidateDescriptionLength()
    {
        // Arrange
        var request = new CreateWorkflowRequest
        {
            Name = "Test Workflow",
            Description = new string('A', 1001), // Exceeds max length of 1000
            DocumentId = Guid.NewGuid(),
            WorkflowType = "Test"
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        validationResults.Should().Contain(v => v.ErrorMessage!.Contains("maximum length"));
    }

    [Fact]
    public void CreateWorkflowStepRequest_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var stepRequest = new CreateWorkflowStepRequest();

        // Assert
        stepRequest.StepName.Should().BeEmpty();
        stepRequest.AssignedToUserId.Should().BeEmpty();
        stepRequest.DueDate.Should().BeNull();
    }

    [Fact]
    public void CreateWorkflowStepRequest_ShouldSetProperties()
    {
        // Arrange
        var assignedToUserId = Guid.NewGuid();
        var dueDate = DateTime.UtcNow.AddDays(3);

        // Act
        var stepRequest = new CreateWorkflowStepRequest
        {
            StepName = "Final Review",
            AssignedToUserId = assignedToUserId,
            DueDate = dueDate
        };

        // Assert
        stepRequest.StepName.Should().Be("Final Review");
        stepRequest.AssignedToUserId.Should().Be(assignedToUserId);
        stepRequest.DueDate.Should().Be(dueDate);
    }

    [Fact]
    public void CreateWorkflowStepRequest_ShouldValidateRequiredFields()
    {
        // Arrange
        var stepRequest = new CreateWorkflowStepRequest();

        // Act
        var validationResults = ValidateModel(stepRequest);

        // Assert - StepName is required (string property with Required attribute)
        // AssignedToUserId is a Guid, which has a default value of Guid.Empty, so validation might not trigger
        validationResults.Should().HaveCountGreaterThan(0);
        var errorMessages = validationResults.Select(v => v.ErrorMessage).ToList();
        errorMessages.Should().ContainMatch("*required*");
    }

    private static List<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model);
        Validator.TryValidateObject(model, validationContext, validationResults, true);
        return validationResults;
    }
}