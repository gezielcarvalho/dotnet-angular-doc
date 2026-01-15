using Backend.Models.DTO.Documents;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using System.ComponentModel.DataAnnotations;

namespace backend.tests.Models.DTO.Documents;

public class UploadDocumentVersionRequestTests
{
    [Fact]
    public void UploadDocumentVersionRequest_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var request = new UploadDocumentVersionRequest();

        // Assert
        request.File.Should().BeNull();
        request.ChangeComment.Should().BeNull();
    }

    [Fact]
    public void UploadDocumentVersionRequest_ShouldSetProperties()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        var changeComment = "Updated document content";

        // Act
        var request = new UploadDocumentVersionRequest
        {
            File = mockFile.Object,
            ChangeComment = changeComment
        };

        // Assert
        request.File.Should().Be(mockFile.Object);
        request.ChangeComment.Should().Be(changeComment);
    }

    [Fact]
    public void UploadDocumentVersionRequest_ShouldSetPropertiesWithNullComment()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();

        // Act
        var request = new UploadDocumentVersionRequest
        {
            File = mockFile.Object,
            ChangeComment = null
        };

        // Assert
        request.File.Should().Be(mockFile.Object);
        request.ChangeComment.Should().BeNull();
    }

    [Fact]
    public void UploadDocumentVersionRequest_ShouldValidateRequiredFile()
    {
        // Arrange
        var request = new UploadDocumentVersionRequest
        {
            File = null, // Null file should fail validation
            ChangeComment = "Test comment"
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        validationResults.Should().ContainSingle()
            .Which.ErrorMessage.Should().Contain("required");
    }

    [Fact]
    public void UploadDocumentVersionRequest_ShouldValidateChangeCommentLength()
    {
        // Arrange
        var longComment = new string('a', 501); // Exceeds max length of 500
        var request = new UploadDocumentVersionRequest
        {
            File = new Mock<IFormFile>().Object,
            ChangeComment = longComment
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        validationResults.Should().ContainSingle()
            .Which.ErrorMessage.Should().Contain("maximum length");
    }

    [Fact]
    public void UploadDocumentVersionRequest_ShouldPassValidationWithValidData()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        var request = new UploadDocumentVersionRequest
        {
            File = mockFile.Object,
            ChangeComment = "Valid change comment"
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void UploadDocumentVersionRequest_ShouldPassValidationWithNullComment()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        var request = new UploadDocumentVersionRequest
        {
            File = mockFile.Object,
            ChangeComment = null
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