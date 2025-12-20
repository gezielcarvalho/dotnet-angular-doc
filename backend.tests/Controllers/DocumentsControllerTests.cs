using backend.tests.Fixtures;
using backend.tests.Helpers;
using Backend.Controllers;
using Backend.Data;
using Backend.Models.Document;
using Backend.Models.DTO.Common;
using Backend.Models.DTO.Documents;
using Backend.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace backend.tests.Controllers;

public class DocumentsControllerTests
{
    [Fact]
    public async Task GetDocuments_WithFolderId_ForbidWhenNoAccess()
    {
        var context = DbContextHelper.GetInMemoryDbContext();
        var user = EdmFixtures.GetTestUser();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var perm = new Mock<IPermissionService>();
        perm.Setup(p => p.CanAccessFolderAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), "Read")).ReturnsAsync(false);
        var storage = new Mock<IFileStorageService>();

        var controller = new DocumentsController(context, perm.Object, storage.Object);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) })) } };

        var result = await controller.GetDocuments(Guid.NewGuid(), new Backend.Models.DTO.Common.PaginationFilter { PageNumber = 1, PageSize = 10 });

        result.Result.Should().BeOfType<ForbidResult>();

        DbContextHelper.CleanupDbContext(context);
    }

    [Fact]
    public async Task GetDocument_NotFound_ReturnsNotFound()
    {
        var context = DbContextHelper.GetInMemoryDbContext();
        var user = EdmFixtures.GetTestUser();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var perm = new Mock<IPermissionService>();
        perm.Setup(p => p.CanAccessDocumentAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), "Read")).ReturnsAsync(true);
        var storage = new Mock<IFileStorageService>();

        var controller = new DocumentsController(context, perm.Object, storage.Object);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) })) } };

        var result = await controller.GetDocument(Guid.NewGuid());

        result.Result.Should().BeOfType<NotFoundObjectResult>();

        DbContextHelper.CleanupDbContext(context);
    }

    [Fact]
    public async Task CreateDocument_FileRequired_ReturnsBadRequest()
    {
        var context = DbContextHelper.GetInMemoryDbContext();
        var user = EdmFixtures.GetTestUser();
        var folder = EdmFixtures.GetTestFolder(user.Id);
        context.Users.Add(user);
        context.Folders.Add(folder);
        await context.SaveChangesAsync();

        var perm = new Mock<IPermissionService>();
        perm.Setup(p => p.CanAccessFolderAsync(It.IsAny<Guid>(), folder.Id, "Write")).ReturnsAsync(true);
        var storage = new Mock<IFileStorageService>();

        var controller = new DocumentsController(context, perm.Object, storage.Object);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), new Claim(ClaimTypes.Name, "testuser") })) } };

        var req = new CreateDocumentRequest { Title = "t", Description = "d", FolderId = folder.Id, File = null };
        var result = await controller.CreateDocument(req);

        result.Result.Should().BeOfType<BadRequestObjectResult>();

        DbContextHelper.CleanupDbContext(context);
    }

    [Fact]
    public async Task DownloadDocument_FileMissing_ReturnsNotFound()
    {
        var context = DbContextHelper.GetInMemoryDbContext();
        var user = EdmFixtures.GetTestUser();
        var folder = EdmFixtures.GetTestFolder(user.Id);
        var doc = EdmFixtures.GetTestDocument(folder.Id, user.Id);
        // Ensure FilePath set so controller attempts to fetch
        doc.FilePath = "some/path";
        context.Users.Add(user);
        context.Folders.Add(folder);
        context.Documents.Add(doc);
        await context.SaveChangesAsync();

        var perm = new Mock<IPermissionService>();
        perm.Setup(p => p.CanAccessDocumentAsync(It.IsAny<Guid>(), doc.Id, "Read")).ReturnsAsync(true);
        var storage = new Mock<IFileStorageService>();
        storage.Setup(s => s.GetFileAsync(It.IsAny<string>())).ReturnsAsync((System.IO.Stream?)null);

        var controller = new DocumentsController(context, perm.Object, storage.Object);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) })) } };

        var result = await controller.DownloadDocument(doc.Id, null);

        result.Should().BeOfType<NotFoundResult>();

        DbContextHelper.CleanupDbContext(context);
    }

    [Fact]
    public async Task GetDocumentVersions_ReturnsVersions()
    {
        var context = DbContextHelper.GetInMemoryDbContext();
        var user = EdmFixtures.GetTestUser();
        var folder = EdmFixtures.GetTestFolder(user.Id);
        var doc = EdmFixtures.GetTestDocument(folder.Id, user.Id);

        var version = new DocumentVersion
        {
            Id = Guid.NewGuid(),
            DocumentId = doc.Id,
            VersionNumber = 1,
            FileName = "f.txt",
            FilePath = "p",
            FileSizeBytes = 100,
            ChangeComment = "init",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "t"
        };

        context.Users.Add(user);
        context.Folders.Add(folder);
        context.Documents.Add(doc);
        context.DocumentVersions.Add(version);
        await context.SaveChangesAsync();

        var perm = new Mock<IPermissionService>();
        perm.Setup(p => p.CanAccessDocumentAsync(It.IsAny<Guid>(), doc.Id, "Read")).ReturnsAsync(true);
        var storage = new Mock<IFileStorageService>();

        var controller = new DocumentsController(context, perm.Object, storage.Object);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) })) } };

        var result = await controller.GetDocumentVersions(doc.Id);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var api = ok.Value as ApiResponse<List<DocumentVersionDTO>>;
        api.Should().NotBeNull();
        api!.Data.Should().HaveCount(1);

        DbContextHelper.CleanupDbContext(context);
    }
}
