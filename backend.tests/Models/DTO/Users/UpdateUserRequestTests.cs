using Backend.Models.DTO.Users;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;

namespace backend.tests.Models.DTO.Users;

public class UpdateUserRequestTests
{
    [Fact]
    public void UpdateUserRequest_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var request = new UpdateUserRequest();

        // Assert
        request.Email.Should().BeEmpty();
        request.FirstName.Should().BeEmpty();
        request.LastName.Should().BeEmpty();
        request.Role.Should().BeEmpty();
        request.Department.Should().BeNull();
        request.IsActive.Should().BeFalse();
    }

    [Fact]
    public void UpdateUserRequest_ShouldSetProperties()
    {
        // Arrange
        var request = new UpdateUserRequest
        {
            Email = "john.doe@example.com",
            FirstName = "John",
            LastName = "Doe",
            Role = "Admin",
            Department = "IT",
            IsActive = true
        };

        // Assert
        request.Email.Should().Be("john.doe@example.com");
        request.FirstName.Should().Be("John");
        request.LastName.Should().Be("Doe");
        request.Role.Should().Be("Admin");
        request.Department.Should().Be("IT");
        request.IsActive.Should().BeTrue();
    }

    [Fact]
    public void UpdateUserRequest_ShouldSetPropertiesWithNullDepartment()
    {
        // Arrange
        var request = new UpdateUserRequest
        {
            Email = "jane.smith@example.com",
            FirstName = "Jane",
            LastName = "Smith",
            Role = "User",
            Department = null,
            IsActive = false
        };

        // Assert
        request.Email.Should().Be("jane.smith@example.com");
        request.FirstName.Should().Be("Jane");
        request.LastName.Should().Be("Smith");
        request.Role.Should().Be("User");
        request.Department.Should().BeNull();
        request.IsActive.Should().BeFalse();
    }

    [Fact]
    public void UpdateUserRequest_ShouldValidateRequiredEmail()
    {
        // Arrange
        var request = new UpdateUserRequest
        {
            Email = "", // Empty email should fail validation
            FirstName = "John",
            LastName = "Doe",
            Role = "User"
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        validationResults.Should().ContainSingle()
            .Which.ErrorMessage.Should().Contain("required");
    }

    [Fact]
    public void UpdateUserRequest_ShouldValidateEmailFormat()
    {
        // Arrange
        var request = new UpdateUserRequest
        {
            Email = "invalid-email", // Invalid email format
            FirstName = "John",
            LastName = "Doe",
            Role = "User"
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        validationResults.Should().ContainSingle()
            .Which.ErrorMessage.Should().Contain("valid e-mail address");
    }

    [Fact]
    public void UpdateUserRequest_ShouldValidateRequiredFirstName()
    {
        // Arrange
        var request = new UpdateUserRequest
        {
            Email = "john.doe@example.com",
            FirstName = "", // Empty first name should fail validation
            LastName = "Doe",
            Role = "User"
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        validationResults.Should().ContainSingle()
            .Which.ErrorMessage.Should().Contain("required");
    }

    [Fact]
    public void UpdateUserRequest_ShouldValidateRequiredLastName()
    {
        // Arrange
        var request = new UpdateUserRequest
        {
            Email = "john.doe@example.com",
            FirstName = "John",
            LastName = "", // Empty last name should fail validation
            Role = "User"
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        validationResults.Should().ContainSingle()
            .Which.ErrorMessage.Should().Contain("required");
    }

    [Fact]
    public void UpdateUserRequest_ShouldValidateRequiredRole()
    {
        // Arrange
        var request = new UpdateUserRequest
        {
            Email = "john.doe@example.com",
            FirstName = "John",
            LastName = "Doe",
            Role = "" // Empty role should fail validation
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        validationResults.Should().ContainSingle()
            .Which.ErrorMessage.Should().Contain("required");
    }

    [Fact]
    public void UpdateUserRequest_ShouldPassValidationWithValidData()
    {
        // Arrange
        var request = new UpdateUserRequest
        {
            Email = "john.doe@example.com",
            FirstName = "John",
            LastName = "Doe",
            Role = "Admin",
            Department = "IT",
            IsActive = true
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