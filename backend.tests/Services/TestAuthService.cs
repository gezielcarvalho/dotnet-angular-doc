using backend.tests.Fixtures;
using backend.tests.Helpers;
using Backend.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace backend.tests.Services;

public class TestAuthService
{
    private readonly IConfiguration _configuration;

    public TestAuthService()
    {
        var inMemorySettings = new Dictionary<string, string>
        {
            {"JwtSettings:SecretKey", "SuperSecretKeyForTestingPurposesOnly123456789"},
            {"JwtSettings:Issuer", "TestIssuer"},
            {"JwtSettings:Audience", "TestAudience"},
            {"JwtSettings:ExpirationMinutes", "60"}
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsSuccessWithToken()
    {
        // Arrange
        var context = DbContextHelper.GetInMemoryDbContext();
        var user = EdmFixtures.GetTestUser();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = new AuthService(context, _configuration, new Backend.Services.NullEmailService(), Microsoft.Extensions.Logging.Abstractions.NullLogger<AuthService>.Instance);
        var loginRequest = EdmFixtures.GetLoginRequest();

        // Act
        var result = await service.LoginAsync(loginRequest);

        // Assert
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
        result.User.Should().NotBeNull();
        result.User.Username.Should().Be(user.Username);
        result.User.Email.Should().Be(user.Email);

        DbContextHelper.CleanupDbContext(context);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidUsername_ReturnsNull()
    {
        // Arrange
        var context = DbContextHelper.GetInMemoryDbContext();
        var service = new AuthService(context, _configuration);
        var loginRequest = new Backend.Models.DTO.Auth.LoginRequest
        {
            Username = "nonexistent",
            Password = "Test@123"
        };

        // Act
        var result = await service.LoginAsync(loginRequest);

        // Assert
        result.Should().BeNull();

        DbContextHelper.CleanupDbContext(context);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ReturnsNull()
    {
        // Arrange
        var context = DbContextHelper.GetInMemoryDbContext();
        var user = EdmFixtures.GetTestUser();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = new AuthService(context, _configuration);
        var loginRequest = new Backend.Models.DTO.Auth.LoginRequest
        {
            Username = "testuser",
            Password = "WrongPassword"
        };

        // Act
        var result = await service.LoginAsync(loginRequest);

        // Assert
        result.Should().BeNull();

        DbContextHelper.CleanupDbContext(context);
    }

    [Fact]
    public async Task LoginAsync_WithInactiveUser_ReturnsNull()
    {
        // Arrange
        var context = DbContextHelper.GetInMemoryDbContext();
        var user = EdmFixtures.GetTestUser();
        user.IsActive = false;
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = new AuthService(context, _configuration);
        var loginRequest = EdmFixtures.GetLoginRequest();

        // Act
        var result = await service.LoginAsync(loginRequest);

        // Assert
        result.Should().BeNull();

        DbContextHelper.CleanupDbContext(context);
    }

    [Fact]
    public async Task RegisterAsync_WithValidData_CreatesUser()
    {
        // Arrange
        var context = DbContextHelper.GetInMemoryDbContext();
        var service = new AuthService(context, _configuration);
        var registerRequest = EdmFixtures.GetRegisterRequest();

        // Act
        var result = await service.RegisterAsync(registerRequest);

        // Assert
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
        result.User.Should().NotBeNull();
        result.User.Username.Should().Be(registerRequest.Username);
        result.User.Email.Should().Be(registerRequest.Email);
        result.User.Role.Should().Be("User"); // Default role

        var savedUser = await context.Users.FindAsync(result.User.Id);
        savedUser.Should().NotBeNull();
        savedUser!.PasswordHash.Should().NotBe(registerRequest.Password); // Password should be hashed

        DbContextHelper.CleanupDbContext(context);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingUsername_ReturnsNull()
    {
        // Arrange
        var context = DbContextHelper.GetInMemoryDbContext();
        var existingUser = EdmFixtures.GetTestUser();
        context.Users.Add(existingUser);
        await context.SaveChangesAsync();

        var service = new AuthService(context, _configuration);
        var registerRequest = new Backend.Models.DTO.Auth.RegisterRequest
        {
            Username = "testuser", // Same as existing user
            Email = "different@example.com",
            Password = "NewUser@123",
            FirstName = "New",
            LastName = "User"
        };

        // Act
        var result = await service.RegisterAsync(registerRequest);

        // Assert
        result.Should().BeNull();

        DbContextHelper.CleanupDbContext(context);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ReturnsNull()
    {
        // Arrange
        var context = DbContextHelper.GetInMemoryDbContext();
        var existingUser = EdmFixtures.GetTestUser();
        context.Users.Add(existingUser);
        await context.SaveChangesAsync();

        var service = new AuthService(context, _configuration);
        var registerRequest = new Backend.Models.DTO.Auth.RegisterRequest
        {
            Username = "differentuser",
            Email = "test@example.com", // Same as existing user
            Password = "NewUser@123",
            FirstName = "New",
            LastName = "User"
        };

        // Act
        var result = await service.RegisterAsync(registerRequest);

        // Assert
        result.Should().BeNull();

        DbContextHelper.CleanupDbContext(context);
    }

    [Fact]
    public async Task ChangePasswordAsync_WithValidCurrentPassword_ChangesPassword()
    {
        // Arrange
        var context = DbContextHelper.GetInMemoryDbContext();
        var user = EdmFixtures.GetTestUser();
        var originalPasswordHash = user.PasswordHash;
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = new AuthService(context, _configuration);
        var changePasswordRequest = new Backend.Models.DTO.Auth.ChangePasswordRequest
        {
            CurrentPassword = "Test@123",
            NewPassword = "NewPassword@123",
            ConfirmPassword = "NewPassword@123"
        };

        // Act
        var result = await service.ChangePasswordAsync(user.Id, changePasswordRequest);

        // Assert
        result.Should().BeTrue();

        var updatedUser = await context.Users.FindAsync(user.Id);
        updatedUser!.PasswordHash.Should().NotBe(originalPasswordHash);
        BCrypt.Net.BCrypt.Verify("NewPassword@123", updatedUser.PasswordHash).Should().BeTrue();

        DbContextHelper.CleanupDbContext(context);
    }

    [Fact]
    public async Task ChangePasswordAsync_WithInvalidCurrentPassword_ReturnsFalse()
    {
        // Arrange
        var context = DbContextHelper.GetInMemoryDbContext();
        var user = EdmFixtures.GetTestUser();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = new AuthService(context, _configuration);
        var changePasswordRequest = new Backend.Models.DTO.Auth.ChangePasswordRequest
        {
            CurrentPassword = "WrongPassword",
            NewPassword = "NewPassword@123",
            ConfirmPassword = "NewPassword@123"
        };

        // Act
        var result = await service.ChangePasswordAsync(user.Id, changePasswordRequest);

        // Assert
        result.Should().BeFalse();

        DbContextHelper.CleanupDbContext(context);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithValidId_ReturnsUser()
    {
        // Arrange
        var context = DbContextHelper.GetInMemoryDbContext();
        var user = EdmFixtures.GetTestUser();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = new AuthService(context, _configuration);

        // Act
        var result = await service.GetUserByIdAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Username.Should().Be(user.Username);

        DbContextHelper.CleanupDbContext(context);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var context = DbContextHelper.GetInMemoryDbContext();
        var service = new AuthService(context, _configuration);

        // Act
        var result = await service.GetUserByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();

        DbContextHelper.CleanupDbContext(context);
    }

    [Fact]
    public async Task GetUserByUsernameAsync_WithValidUsername_ReturnsUser()
    {
        // Arrange
        var context = DbContextHelper.GetInMemoryDbContext();
        var user = EdmFixtures.GetTestUser();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = new AuthService(context, _configuration);

        // Act
        var result = await service.GetUserByUsernameAsync(user.Username);

        // Assert
        result.Should().NotBeNull();
        result!.Username.Should().Be(user.Username);

        DbContextHelper.CleanupDbContext(context);
    }

    [Fact]
    public void GenerateJwtToken_WithValidUser_ReturnsToken()
    {
        // Arrange
        var context = DbContextHelper.GetInMemoryDbContext();
        var service = new AuthService(context, _configuration);
        var user = EdmFixtures.GetTestUser();

        // Act
        var token = service.GenerateJwtToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();
        token.Split('.').Should().HaveCount(3); // JWT has 3 parts

        DbContextHelper.CleanupDbContext(context);
    }
}
