using Backend.Controllers;
using Backend.Models.DTO.Auth;
using Backend.Models.DTO.Common;
using Backend.Models.Document;
using Backend.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Security.Claims;

namespace backend.tests.Controllers;

public class AuthControllerTests
{
    [Fact]
    public async Task RequestPasswordReset_ReturnsOk()
    {
        var mock = new Mock<IAuthService>();
        mock.Setup(s => s.RequestPasswordResetAsync("a@b.com", It.IsAny<string>())).ReturnsAsync(true);

        var controller = new AuthController(mock.Object);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        var req = new RequestPasswordResetRequest { Email = "a@b.com" };

        var result = await controller.RequestPasswordReset(req);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var api = ok.Value as ApiResponse<bool>;
        api.Should().NotBeNull();
        api!.Data.Should().BeTrue();
    }

    [Fact]
    public async Task RequestPasswordReset_UsesRequestOrigin_WhenProvided()
    {
        var mock = new Mock<IAuthService>();
        mock.Setup(s => s.RequestPasswordResetAsync("a@b.com", "explicit-origin")).ReturnsAsync(true);

        var controller = new AuthController(mock.Object);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        var req = new RequestPasswordResetRequest { Email = "a@b.com", Origin = "explicit-origin" };

        var result = await controller.RequestPasswordReset(req);

        result.Result.Should().BeOfType<OkObjectResult>();
        mock.Verify(s => s.RequestPasswordResetAsync("a@b.com", "explicit-origin"), Times.Once);
    }

    [Fact]
    public async Task PreviewPasswordReset_ReturnsOk()
    {
        var mock = new Mock<IAuthService>();
        mock.Setup(s => s.GeneratePasswordResetEmailPreviewAsync("a@b.com", It.IsAny<string>())).ReturnsAsync("body");

        var controller = new AuthController(mock.Object);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        var req = new RequestPasswordResetRequest { Email = "a@b.com" };

        var result = await controller.PreviewPasswordReset(req);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var api = ok.Value as ApiResponse<string>;
        api.Should().NotBeNull();
        api!.Data.Should().Be("body");
    }

    [Fact]
    public async Task PreviewPasswordReset_UsesHeaderOrigin_WhenRequestOriginNull()
    {
        var mock = new Mock<IAuthService>();
        mock.Setup(s => s.GeneratePasswordResetEmailPreviewAsync("a@b.com", "header-origin")).ReturnsAsync("body");

        var controller = new AuthController(mock.Object);
        var ctx = new DefaultHttpContext();
        ctx.Request.Headers["Origin"] = "header-origin";
        controller.ControllerContext = new ControllerContext { HttpContext = ctx };

        var req = new RequestPasswordResetRequest { Email = "a@b.com", Origin = null };
        var result = await controller.PreviewPasswordReset(req);

        result.Result.Should().BeOfType<OkObjectResult>();
        mock.Verify(s => s.GeneratePasswordResetEmailPreviewAsync("a@b.com", "header-origin"), Times.Once);
    }

    [Fact]
    public async Task RequestPasswordReset_UsesHeaderOrigin_WhenRequestOriginNull()
    {
        var mock = new Mock<IAuthService>();
        mock.Setup(s => s.RequestPasswordResetAsync("a@b.com", "header-origin")).ReturnsAsync(true);

        var controller = new AuthController(mock.Object);
        var ctx = new DefaultHttpContext();
        ctx.Request.Headers["Origin"] = "header-origin";
        controller.ControllerContext = new ControllerContext { HttpContext = ctx };

        var req = new RequestPasswordResetRequest { Email = "a@b.com", Origin = null };
        var result = await controller.RequestPasswordReset(req);

        result.Result.Should().BeOfType<OkObjectResult>();
        mock.Verify(s => s.RequestPasswordResetAsync("a@b.com", "header-origin"), Times.Once);
    }

    [Fact]
    public async Task ResetPassword_Fails_ReturnsBadRequest()
    {
        var mock = new Mock<IAuthService>();
        mock.Setup(s => s.ResetPasswordAsync("t", "p")).ReturnsAsync(false);

        var controller = new AuthController(mock.Object);
        var req = new ResetPasswordRequest { Token = "t", NewPassword = "p" };

        var result = await controller.ResetPassword(req);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ResetPassword_Succeeds_ReturnsOk()
    {
        var mock = new Mock<IAuthService>();
        mock.Setup(s => s.ResetPasswordAsync("t", "p")).ReturnsAsync(true);

        var controller = new AuthController(mock.Object);
        var req = new ResetPasswordRequest { Token = "t", NewPassword = "p" };

        var result = await controller.ResetPassword(req);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Login_Invalid_ReturnsUnauthorized()
    {
        var mock = new Mock<IAuthService>();
        mock.Setup(s => s.LoginAsync(It.IsAny<LoginRequest>())).ReturnsAsync((LoginResponse?)null);

        var controller = new AuthController(mock.Object);
        var req = new LoginRequest { Username = "u", Password = "p" };

        var result = await controller.Login(req);

        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Login_Valid_ReturnsOk()
    {
        var mock = new Mock<IAuthService>();
        var resp = new LoginResponse { Token = "t" };
        mock.Setup(s => s.LoginAsync(It.IsAny<LoginRequest>())).ReturnsAsync(resp);

        var controller = new AuthController(mock.Object);
        var req = new LoginRequest { Username = "u", Password = "p" };

        var result = await controller.Login(req);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var api = ok.Value as ApiResponse<LoginResponse>;
        api.Should().NotBeNull();
        api!.Data.Token.Should().Be("t");
    }

    [Fact]
    public async Task Register_Invalid_ReturnsBadRequest()
    {
        var mock = new Mock<IAuthService>();
        mock.Setup(s => s.RegisterAsync(It.IsAny<RegisterRequest>())).ReturnsAsync((LoginResponse?)null);

        var controller = new AuthController(mock.Object);
        var req = new RegisterRequest { Username = "u" };

        var result = await controller.Register(req);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Register_Valid_ReturnsOk()
    {
        var mock = new Mock<IAuthService>();
        var resp = new LoginResponse { Token = "t" };
        mock.Setup(s => s.RegisterAsync(It.IsAny<RegisterRequest>())).ReturnsAsync(resp);

        var controller = new AuthController(mock.Object);
        var req = new RegisterRequest { Username = "u" };

        var result = await controller.Register(req);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var api = ok.Value as ApiResponse<LoginResponse>;
        api.Should().NotBeNull();
        api!.Data.Token.Should().Be("t");
    }

    [Fact]
    public async Task ChangePassword_Invalid_ReturnsBadRequest()
    {
        var mock = new Mock<IAuthService>();
        mock.Setup(s => s.ChangePasswordAsync(It.IsAny<Guid>(), It.IsAny<ChangePasswordRequest>())).ReturnsAsync(false);

        var controller = new AuthController(mock.Object);
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()) };
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims)) } };

        var req = new ChangePasswordRequest { CurrentPassword = "a", NewPassword = "b" };
        var result = await controller.ChangePassword(req);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ChangePassword_Valid_ReturnsOk()
    {
        var mock = new Mock<IAuthService>();
        mock.Setup(s => s.ChangePasswordAsync(It.IsAny<Guid>(), It.IsAny<ChangePasswordRequest>())).ReturnsAsync(true);

        var controller = new AuthController(mock.Object);
        var userId = Guid.NewGuid();
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims)) } };

        var req = new ChangePasswordRequest { CurrentPassword = "a", NewPassword = "b" };
        var result = await controller.ChangePassword(req);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetCurrentUser_NotFound_ReturnsNotFound()
    {
        var mock = new Mock<IAuthService>();
        mock.Setup(s => s.GetUserByIdAsync(It.IsAny<Guid>())).ReturnsAsync((User?)null);

        var controller = new AuthController(mock.Object);
        var userId = Guid.NewGuid();
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims)) } };

        var result = await controller.GetCurrentUser();

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetCurrentUser_ReturnsOk()
    {
        var mock = new Mock<IAuthService>();
        var user = new User { Id = Guid.NewGuid(), Username = "u", Email = "e", FirstName = "F", LastName = "L", Role = "User" };
        mock.Setup(s => s.GetUserByIdAsync(It.IsAny<Guid>())).ReturnsAsync(user);

        var controller = new AuthController(mock.Object);
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) };
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims)) } };

        var result = await controller.GetCurrentUser();

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var api = ok.Value as ApiResponse<UserInfo>;
        api.Should().NotBeNull();
        api!.Data.Username.Should().Be("u");
    }
}
