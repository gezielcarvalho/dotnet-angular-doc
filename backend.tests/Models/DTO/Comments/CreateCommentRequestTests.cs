using Backend.Models.DTO.Comments;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;

namespace backend.tests.Models.DTO.Comments;

public class CreateCommentRequestTests
{
    [Fact]
    public void CreateCommentRequest_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var request = new CreateCommentRequest();

        // Assert
        request.Content.Should().BeEmpty();
        request.ParentCommentId.Should().BeNull();
    }

    [Fact]
    public void CreateCommentRequest_ShouldSetProperties()
    {
        // Arrange
        var content = "This is a test comment";
        var parentCommentId = Guid.NewGuid();

        // Act
        var request = new CreateCommentRequest
        {
            Content = content,
            ParentCommentId = parentCommentId
        };

        // Assert
        request.Content.Should().Be(content);
        request.ParentCommentId.Should().Be(parentCommentId);
    }

    [Fact]
    public void CreateCommentRequest_ShouldSetPropertiesWithNullParentId()
    {
        // Arrange
        var content = "Reply comment";

        // Act
        var request = new CreateCommentRequest
        {
            Content = content,
            ParentCommentId = null
        };

        // Assert
        request.Content.Should().Be(content);
        request.ParentCommentId.Should().BeNull();
    }

    [Fact]
    public void CreateCommentRequest_ShouldValidateRequiredContent()
    {
        // Arrange
        var request = new CreateCommentRequest
        {
            Content = "", // Empty content should fail validation
            ParentCommentId = null
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        validationResults.Should().ContainSingle()
            .Which.ErrorMessage.Should().Contain("required");
    }

    [Fact]
    public void CreateCommentRequest_ShouldValidateContentLength()
    {
        // Arrange
        var longContent = new string('a', 2001); // Exceeds max length of 2000
        var request = new CreateCommentRequest
        {
            Content = longContent,
            ParentCommentId = null
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        validationResults.Should().ContainSingle()
            .Which.ErrorMessage.Should().Contain("maximum length");
    }

    [Fact]
    public void CreateCommentRequest_ShouldPassValidationWithValidData()
    {
        // Arrange
        var request = new CreateCommentRequest
        {
            Content = "Valid comment content",
            ParentCommentId = Guid.NewGuid()
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void CreateCommentRequest_ShouldPassValidationWithNullParentId()
    {
        // Arrange
        var request = new CreateCommentRequest
        {
            Content = "Valid comment content",
            ParentCommentId = null
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        validationResults.Should().BeEmpty();
    }

    private static List<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model);
        Validator.TryValidateObject(model, validationContext, validationResults, true);
        return validationResults;
    }
}