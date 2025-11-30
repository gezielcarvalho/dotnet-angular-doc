using backend.tests.Fixtures;
using backend.tests.Helpers;
using Backend.Controllers;
using Backend.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace backend.tests.Controllers;

public class TestAuthController
{
    private readonly IConfiguration _configuration;

    public TestAuthController()
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
    public async Task Login_WithValidCredentials_ReturnsOkWithToken()
    {
        // Arrange
        var context = DbContextHelper.GetInMemoryDbContext();
        var user = EdmFixtures.GetTestUser();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var authService = new AuthService(context, _configuration, new Backend.Services.NullEmailService(), Microsoft.Extensions.Logging.Abstractions.NullLogger<AuthService>.Instance);
        var controller = new AuthController(authService);
        var loginRequest = EdmFixtures.GetLoginRequest();

        // Act
        var result = await controller.Login(loginRequest);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().NotBeNull();

        DbContextHelper.CleanupDbContext(context);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var context = DbContextHelper.GetInMemoryDbContext();
        var authService = new AuthService(context, _configuration);
        var controller = new AuthController(authService);
        var loginRequest = new Backend.Models.DTO.Auth.LoginRequest
        {
            Username = "nonexistent",
            Password = "WrongPassword"
        };

        // Act
        var result = await controller.Login(loginRequest);

        // Assert
        var unauthorizedResult = result.Result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauthorizedResult.StatusCode.Should().Be(401);

        DbContextHelper.CleanupDbContext(context);
    }

    [Fact]
    public async Task Register_WithValidData_ReturnsOkWithToken()
    {
        // Arrange
        var context = DbContextHelper.GetInMemoryDbContext();
        var authService = new AuthService(context, _configuration);
        var controller = new AuthController(authService);
        var registerRequest = EdmFixtures.GetRegisterRequest();

        // Act
        var result = await controller.Register(registerRequest);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().NotBeNull();

        // Verify personal folder created
        var createdUser = await context.Users.FirstOrDefaultAsync(u => u.Username == registerRequest.Username);
        createdUser.Should().NotBeNull();
        var personalFolder = await context.Folders.FirstOrDefaultAsync(f => f.OwnerId == createdUser!.Id && f.ParentFolderId != null);
        personalFolder.Should().NotBeNull();
        var permission = await context.Permissions.FirstOrDefaultAsync(p => p.FolderId == personalFolder!.Id && p.UserId == createdUser.Id && p.PermissionType == "Admin");
        permission.Should().NotBeNull();

        DbContextHelper.CleanupDbContext(context);
    }

    [Fact]
    public async Task Register_WithExistingUsername_ReturnsBadRequest()
    {
        // Arrange
        var context = DbContextHelper.GetInMemoryDbContext();
        var existingUser = EdmFixtures.GetTestUser();
        context.Users.Add(existingUser);
        await context.SaveChangesAsync();

        var authService = new AuthService(context, _configuration);
        var controller = new AuthController(authService);
        var registerRequest = new Backend.Models.DTO.Auth.RegisterRequest
        {
            Username = "testuser", // Same as existing
            Email = "different@example.com",
            Password = "NewUser@123",
            FirstName = "New",
            LastName = "User"
        };

        // Act
        var result = await controller.Register(registerRequest);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);

        DbContextHelper.CleanupDbContext(context);
    }

    [Fact]
    public async Task ChangePassword_WithValidData_ReturnsOk()
    {
        // Arrange
        var context = DbContextHelper.GetInMemoryDbContext();
        var user = EdmFixtures.GetTestUser();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var authService = new AuthService(context, _configuration);
        var controller = new AuthController(authService);

        // Set up HttpContext with user claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        var changePasswordRequest = new Backend.Models.DTO.Auth.ChangePasswordRequest
        {
            CurrentPassword = "Test@123",
            NewPassword = "NewPassword@123",
            ConfirmPassword = "NewPassword@123"
        };

        // Act
        var result = await controller.ChangePassword(changePasswordRequest);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        DbContextHelper.CleanupDbContext(context);
    }

    [Fact]
    public async Task GetCurrentUser_WithValidToken_ReturnsUser()
    {
        // Arrange
        var context = DbContextHelper.GetInMemoryDbContext();
        var user = EdmFixtures.GetTestUser();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var authService = new AuthService(context, _configuration);
        var controller = new AuthController(authService);

        // Set up HttpContext with user claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        // Act
        var result = await controller.GetCurrentUser();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();

        DbContextHelper.CleanupDbContext(context);
    }
}
