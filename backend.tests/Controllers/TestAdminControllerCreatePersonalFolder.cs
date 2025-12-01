using backend.tests.Fixtures;
using backend.tests.Helpers;
using Backend.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace backend.tests.Controllers;

public class TestAdminControllerCreatePersonalFolder
{
    [Fact]
    public async Task CreatePersonalFolderForUser_AsAdmin_CreatesFolder()
    {
        // Arrange
        var context = DbContextHelper.GetInMemoryDbContext();
        var admin = EdmFixtures.GetTestAdmin();
        var user1 = EdmFixtures.GetTestUser();
        context.Users.AddRange(admin, user1);
        await context.SaveChangesAsync();

        var controller = new AdminController(context, NullLogger<AdminController>.Instance);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, admin.Id.ToString()),
            new Claim(ClaimTypes.Role, "Admin")
        };
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth")) }
        };

        // Act
        var result = await controller.CreatePersonalFolderForUser(user1.Id);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        var response = okResult.Value as Backend.Models.DTO.Common.ApiResponse<bool>;
        response.Should().NotBeNull();
        response!.Data.Should().BeTrue();

        // Validate personal folder exists for user1
        var root = await context.Folders.FirstOrDefaultAsync(f => f.ParentFolderId == null);
        var usersFolder = await context.Folders.FirstOrDefaultAsync(f => f.Name == "Users" && f.ParentFolderId == root.Id);
        var personal1 = await context.Folders.FirstOrDefaultAsync(f => f.ParentFolderId == usersFolder!.Id && f.OwnerId == user1.Id);
        personal1.Should().NotBeNull();

        DbContextHelper.CleanupDbContext(context);
    }

    [Fact]
    public async Task CreatePersonalFolderForUser_AsNonAdmin_ReturnsForbid()
    {
        // Arrange
        var context = DbContextHelper.GetInMemoryDbContext();
        var user = EdmFixtures.GetTestUser();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var controller = new AdminController(context, NullLogger<AdminController>.Instance);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, "User")
        };
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth")) }
        };

        // Act
        var result = await controller.CreatePersonalFolderForUser(user.Id);

        // Assert
        var forbidResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        forbidResult.StatusCode.Should().Be(403);

        DbContextHelper.CleanupDbContext(context);
    }
}
