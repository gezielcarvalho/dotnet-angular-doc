using Backend.Controllers;
using Backend.Data;
using Backend.Models.DTO.Common;
using Backend.Models.DTO.Tags;
using Backend.Models.Document;
using backend.tests.Fixtures;
using backend.tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.tests.Controllers;

public class TagsControllerTests
{
    private TagsController CreateController(DocumentDbContext context, string? username = null, string? role = null)
    {
        var controller = new TagsController(context);
        var claims = new List<Claim>();
        if (!string.IsNullOrEmpty(username)) claims.Add(new Claim(ClaimTypes.Name, username));
        if (!string.IsNullOrEmpty(role)) claims.Add(new Claim(ClaimTypes.Role, role));
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims)) } };
        return controller;
    }

    [Fact]
    public async Task GetTags_ReturnsAllTags()
    {
        var context = DbContextHelper.GetInMemoryDbContext();
        var t1 = EdmFixtures.GetTestTag();
        var t2 = EdmFixtures.GetTestTag();
        t2.Name = "Another";
        context.Tags.AddRange(t1, t2);
        await context.SaveChangesAsync();

        var controller = CreateController(context);

        var result = await controller.GetTags();

        var ok = result.Result as OkObjectResult;
        ApiResponse<List<TagDTO>>? api = null;
        if (ok != null)
            api = ok.Value as ApiResponse<List<TagDTO>>;
        else
            api = result.Value as ApiResponse<List<TagDTO>>;
        api.Should().NotBeNull();
        api!.Data.Select(d => d.Name).Should().Contain(t1.Name);
        api!.Data.Select(d => d.Name).Should().Contain(t2.Name);

        DbContextHelper.CleanupDbContext(context);
    }

    [Fact]
    public async Task GetTag_NotFound_ReturnsNotFound()
    {
        var context = DbContextHelper.GetInMemoryDbContext();
        var controller = CreateController(context);

        var result = await controller.GetTag(Guid.NewGuid());

        result.Result.Should().BeOfType<NotFoundObjectResult>();

        DbContextHelper.CleanupDbContext(context);
    }

    [Fact]
    public async Task GetTag_ReturnsTag()
    {
        var context = DbContextHelper.GetInMemoryDbContext();
        var tag = EdmFixtures.GetTestTag();
        context.Tags.Add(tag);
        await context.SaveChangesAsync();

        var controller = CreateController(context);

        var result = await controller.GetTag(tag.Id);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var api = ok.Value as ApiResponse<TagDTO>;
        api.Should().NotBeNull();
        api!.Data.Id.Should().Be(tag.Id);

        DbContextHelper.CleanupDbContext(context);
    }

    [Fact]
    public async Task CreateTag_Succeeds_ReturnsCreated()
    {
        var context = DbContextHelper.GetInMemoryDbContext();
        var controller = CreateController(context, username: "creator", role: "Admin");

        var req = new CreateTagRequest { Name = "NewTag", Description = "d", Color = "#000" };

        var result = await controller.CreateTag(req);

        var created = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var api = created.Value as ApiResponse<TagDTO>;
        api.Should().NotBeNull();
        api!.Data.Name.Should().Be("NewTag");

        DbContextHelper.CleanupDbContext(context);
    }

    [Fact]
    public async Task CreateTag_DuplicateName_ReturnsBadRequest()
    {
        var context = DbContextHelper.GetInMemoryDbContext();
        var existing = EdmFixtures.GetTestTag();
        existing.Name = "Dup";
        context.Tags.Add(existing);
        await context.SaveChangesAsync();

        var controller = CreateController(context, username: "creator", role: "Admin");
        var req = new CreateTagRequest { Name = "Dup", Description = "d", Color = "#000" };

        var result = await controller.CreateTag(req);

        result.Result.Should().BeOfType<BadRequestObjectResult>();

        DbContextHelper.CleanupDbContext(context);
    }

    [Fact]
    public async Task UpdateTag_NotFound_ReturnsNotFound()
    {
        var context = DbContextHelper.GetInMemoryDbContext();
        var controller = CreateController(context, username: "u", role: "Admin");

        var req = new CreateTagRequest { Name = "X", Description = "d", Color = "#0" };
        var result = await controller.UpdateTag(Guid.NewGuid(), req);

        result.Result.Should().BeOfType<NotFoundObjectResult>();

        DbContextHelper.CleanupDbContext(context);
    }

    [Fact]
    public async Task UpdateTag_NameConflict_ReturnsBadRequest()
    {
        var context = DbContextHelper.GetInMemoryDbContext();
        var t1 = EdmFixtures.GetTestTag();
        t1.Name = "A";
        var t2 = EdmFixtures.GetTestTag();
        t2.Name = "B";
        context.Tags.AddRange(t1, t2);
        await context.SaveChangesAsync();

        var controller = CreateController(context, username: "u", role: "Admin");
        var req = new CreateTagRequest { Name = "B", Description = "d", Color = "#0" };
        var result = await controller.UpdateTag(t1.Id, req);

        result.Result.Should().BeOfType<BadRequestObjectResult>();

        DbContextHelper.CleanupDbContext(context);
    }

    [Fact]
    public async Task UpdateTag_Succeeds_ReturnsOk()
    {
        var context = DbContextHelper.GetInMemoryDbContext();
        var tag = EdmFixtures.GetTestTag();
        context.Tags.Add(tag);
        await context.SaveChangesAsync();

        var controller = CreateController(context, username: "u", role: "Admin");
        var req = new CreateTagRequest { Name = "Updated", Description = "desc", Color = "#FFF" };
        var result = await controller.UpdateTag(tag.Id, req);

        // verify persisted change
        var updated = await context.Tags.FindAsync(tag.Id);
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("Updated");

        DbContextHelper.CleanupDbContext(context);
    }

    [Fact]
    public async Task DeleteTag_NotFound_ReturnsNotFound()
    {
        var context = DbContextHelper.GetInMemoryDbContext();
        var controller = CreateController(context, username: "u", role: "Admin");

        var result = await controller.DeleteTag(Guid.NewGuid());

        result.Result.Should().BeOfType<NotFoundObjectResult>();

        DbContextHelper.CleanupDbContext(context);
    }

    [Fact]
    public async Task DeleteTag_Succeeds_ReturnsOk()
    {
        var context = DbContextHelper.GetInMemoryDbContext();
        var tag = EdmFixtures.GetTestTag();
        context.Tags.Add(tag);
        await context.SaveChangesAsync();

        var controller = CreateController(context, username: "u", role: "Admin");

        var result = await controller.DeleteTag(tag.Id);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var api = ok.Value as ApiResponse<bool>;
        api.Should().NotBeNull();
        api!.Data.Should().BeTrue();

        DbContextHelper.CleanupDbContext(context);
    }
}
