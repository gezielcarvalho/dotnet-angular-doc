using backend.tests.Fixtures;
using backend.tests.Helpers;
using Backend.Controllers;
using Backend.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace backend.tests.Controllers;

public class TestFoldersController
{
    [Fact]
    public async Task GetFolders_WithRequiredPermissionWrite_ReturnsWritableOnly()
    {
        // Arrange
        var context = DbContextHelper.GetInMemoryDbContext();
        var admin = EdmFixtures.GetTestSystemAdmin();
        var editor = EdmFixtures.GetTestUser("Editor");
        var viewer = EdmFixtures.GetTestUser("Viewer");

        var root = EdmFixtures.GetTestFolder(admin.Id);
        root.IsSystemFolder = true;
        context.Users.AddRange(admin, editor, viewer);
        context.Folders.Add(root);
        await context.SaveChangesAsync();

        var permissionService = new PermissionService(context);
        var controller = new FoldersController(context, permissionService);

        // Set up HttpContext with editor claims
        var editorClaims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, editor.Id.ToString())
        };
        var editorIdentity = new ClaimsIdentity(editorClaims, "TestAuth");
        var editorPrincipal = new ClaimsPrincipal(editorIdentity);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = editorPrincipal }
        };

        // Act
        var editorResult = await controller.GetFolders(null, "Write");

        // Assert
        var okResult = editorResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        var response = okResult.Value as Backend.Models.DTO.Common.ApiResponse<List<Backend.Models.DTO.Folders.FolderDTO>>;
        response.Should().NotBeNull();
        response!.Data.Should().HaveCountGreaterThan(0);
        // We expect at least one folder and that the editor has write permission on at least one
        response.Data.Any(f => f.CanWrite).Should().BeTrue();

        // Now viewer should not see root for Write
        var viewerController = new FoldersController(context, permissionService);
        var viewerClaims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, viewer.Id.ToString())
        };
        var viewerIdentity = new ClaimsIdentity(viewerClaims, "TestAuth");
        var viewerPrincipal = new ClaimsPrincipal(viewerIdentity);
        viewerController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = viewerPrincipal }
        };

        var viewerResult = await viewerController.GetFolders(null, "Write");
        var viewerOk = viewerResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var viewerResponse = viewerOk.Value as Backend.Models.DTO.Common.ApiResponse<List<Backend.Models.DTO.Folders.FolderDTO>>;
        viewerResponse.Should().NotBeNull();
        // viewer should have zero writable folders (Write filter)
        viewerResponse!.Data.Should().HaveCount(0);

        DbContextHelper.CleanupDbContext(context);
    }

    [Fact]
    public async Task GetFolders_WithNoRequiredPermission_Read_ReturnsReadableForAnyUser()
    {
        var context = DbContextHelper.GetInMemoryDbContext();
        var admin = EdmFixtures.GetTestSystemAdmin();
        var viewer = EdmFixtures.GetTestUser("Viewer");
        var root = EdmFixtures.GetTestFolder(admin.Id);
        root.IsSystemFolder = true;
        context.Users.AddRange(admin, viewer);
        context.Folders.Add(root);
        await context.SaveChangesAsync();

        var permissionService = new PermissionService(context);
        var controller = new FoldersController(context, permissionService);

        // Set up HttpContext with viewer claims
        var viewerClaims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, viewer.Id.ToString())
        };
        var viewerIdentity = new ClaimsIdentity(viewerClaims, "TestAuth");
        var viewerPrincipal = new ClaimsPrincipal(viewerIdentity);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = viewerPrincipal }
        };

        var result = await controller.GetFolders(null);
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value as Backend.Models.DTO.Common.ApiResponse<List<Backend.Models.DTO.Folders.FolderDTO>>;
        response.Should().NotBeNull();
        response!.Data.Should().HaveCountGreaterThan(0);
        // viewer should be able to read but not write
        response.Data.All(f => f.CanWrite == false).Should().BeTrue();

        DbContextHelper.CleanupDbContext(context);
    }
}
