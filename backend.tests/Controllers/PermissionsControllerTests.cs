using Backend.Controllers;
using Backend.Models.DTO.Common;
using Backend.Models.DTO.Permissions;
using Backend.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace backend.tests.Controllers;

public class PermissionsControllerTests
{
    private PermissionsController CreateController(Mock<IPermissionService> mockService, Guid currentUserId, string? role = null)
    {
        var controller = new PermissionsController(mockService.Object);
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, currentUserId.ToString()) };
        if (!string.IsNullOrEmpty(role)) claims.Add(new Claim(ClaimTypes.Role, role));
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims)) } };
        return controller;
    }

    [Fact]
    public async Task GetUserPermissions_AsAdmin_ReturnsOk()
    {
        var targetUser = Guid.NewGuid();
        var mock = new Mock<IPermissionService>();
        var sample = new List<PermissionDTO> { new PermissionDTO { Id = Guid.NewGuid(), PermissionType = "Read" } };
        mock.Setup(s => s.GetUserPermissionsAsync(targetUser)).ReturnsAsync(sample);

        var controller = CreateController(mock, Guid.NewGuid(), "Admin");

        var result = await controller.GetUserPermissions(targetUser);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var api = ok.Value as ApiResponse<List<PermissionDTO>>;
        api.Should().NotBeNull();
        api!.Data.Should().BeEquivalentTo(sample);
    }

    [Fact]
    public async Task GetUserPermissions_AsDifferentNonAdmin_ReturnsForbid()
    {
        var targetUser = Guid.NewGuid();
        var currentUser = Guid.NewGuid();
        var mock = new Mock<IPermissionService>();

        var controller = CreateController(mock, currentUser, "User");

        var result = await controller.GetUserPermissions(targetUser);

        result.Result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task GetFolderPermissions_AccessAllowed_ReturnsOk()
    {
        var folderId = Guid.NewGuid();
        var currentUser = Guid.NewGuid();
        var mock = new Mock<IPermissionService>();
        mock.Setup(s => s.CanAccessFolderAsync(currentUser, folderId, "Admin")).ReturnsAsync(true);
        var sample = new List<PermissionDTO> { new PermissionDTO { Id = Guid.NewGuid(), PermissionType = "Admin" } };
        mock.Setup(s => s.GetFolderPermissionsAsync(folderId)).ReturnsAsync(sample);

        var controller = CreateController(mock, currentUser);

        var result = await controller.GetFolderPermissions(folderId);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var api = ok.Value as ApiResponse<List<PermissionDTO>>;
        api.Should().NotBeNull();
        api!.Data.Should().BeEquivalentTo(sample);
    }

    [Fact]
    public async Task GetFolderPermissions_NoAccess_ReturnsForbid()
    {
        var folderId = Guid.NewGuid();
        var currentUser = Guid.NewGuid();
        var mock = new Mock<IPermissionService>();
        mock.Setup(s => s.CanAccessFolderAsync(currentUser, folderId, "Admin")).ReturnsAsync(false);

        var controller = CreateController(mock, currentUser);

        var result = await controller.GetFolderPermissions(folderId);

        result.Result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task GetDocumentPermissions_AccessAllowed_ReturnsOk()
    {
        var docId = Guid.NewGuid();
        var currentUser = Guid.NewGuid();
        var mock = new Mock<IPermissionService>();
        mock.Setup(s => s.CanAccessDocumentAsync(currentUser, docId, "Admin")).ReturnsAsync(true);
        var sample = new List<PermissionDTO> { new PermissionDTO { Id = Guid.NewGuid(), PermissionType = "Admin" } };
        mock.Setup(s => s.GetDocumentPermissionsAsync(docId)).ReturnsAsync(sample);

        var controller = CreateController(mock, currentUser);

        var result = await controller.GetDocumentPermissions(docId);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var api = ok.Value as ApiResponse<List<PermissionDTO>>;
        api.Should().NotBeNull();
        api!.Data.Should().BeEquivalentTo(sample);
    }

    [Fact]
    public async Task GrantPermission_Folder_Succeeds_ReturnsOk()
    {
        var currentUser = Guid.NewGuid();
        var folderId = Guid.NewGuid();
        var request = new CreatePermissionRequest { FolderId = folderId, PermissionType = "Read", UserId = Guid.NewGuid() };

        var granted = new PermissionDTO { Id = Guid.NewGuid(), PermissionType = "Read" };
        var mock = new Mock<IPermissionService>();
        mock.Setup(s => s.CanAccessFolderAsync(currentUser, folderId, "Admin")).ReturnsAsync(true);
        mock.Setup(s => s.GrantPermissionAsync(request, It.IsAny<string>())).ReturnsAsync(granted);

        var controller = CreateController(mock, currentUser);

        var result = await controller.GrantPermission(request);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var api = ok.Value as ApiResponse<PermissionDTO>;
        api.Should().NotBeNull();
        api!.Data.Should().BeEquivalentTo(granted);
    }

    [Fact]
    public async Task GrantPermission_NoResource_ReturnsBadRequest()
    {
        var currentUser = Guid.NewGuid();
        var request = new CreatePermissionRequest { PermissionType = "Read", UserId = Guid.NewGuid() };
        var mock = new Mock<IPermissionService>();
        var controller = CreateController(mock, currentUser);

        var result = await controller.GrantPermission(request);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task RevokePermission_NotFound_ReturnsNotFound()
    {
        var currentUser = Guid.NewGuid();
        var id = Guid.NewGuid();
        var mock = new Mock<IPermissionService>();
        mock.Setup(s => s.RevokePermissionAsync(id)).ReturnsAsync(false);

        var controller = CreateController(mock, currentUser);

        var result = await controller.RevokePermission(id);

        var notFound = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var api = notFound.Value as ApiResponse<bool>;
        api.Should().NotBeNull();
        api!.Data.Should().BeFalse();
    }

    [Fact]
    public async Task RevokePermission_Succeeds_ReturnsOk()
    {
        var currentUser = Guid.NewGuid();
        var id = Guid.NewGuid();
        var mock = new Mock<IPermissionService>();
        mock.Setup(s => s.RevokePermissionAsync(id)).ReturnsAsync(true);

        var controller = CreateController(mock, currentUser);

        var result = await controller.RevokePermission(id);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var api = ok.Value as ApiResponse<bool>;
        api.Should().NotBeNull();
        api!.Data.Should().BeTrue();
    }
}
