using backend.tests.Fixtures;
using backend.tests.Helpers;
using Backend.Services;
using FluentAssertions;

namespace backend.tests.Services;

public class TestPermissionService
{
    [Fact]
    public async Task CanAccessFolderAsync_SystemAdmin_ReturnsTrue()
    {
        // Arrange
        var context = DbContextHelper.GetInMemoryDbContext();
        var admin = EdmFixtures.GetTestSystemAdmin();
        var folder = EdmFixtures.GetTestFolder(admin.Id);
        
        context.Users.Add(admin);
        context.Folders.Add(folder);
        await context.SaveChangesAsync();

        var service = new PermissionService(context);

        // Act
        var result = await service.CanAccessFolderAsync(admin.Id, folder.Id, "Read");

        // Assert
        result.Should().BeTrue();

        DbContextHelper.CleanupDbContext(context);
    }

    [Fact]
    public async Task CanAccessFolderAsync_Owner_ReturnsTrue()
    {
        // Arrange
        var context = DbContextHelper.GetInMemoryDbContext();
        var user = EdmFixtures.GetTestUser();
        var folder = EdmFixtures.GetTestFolder(user.Id);
        
        context.Users.Add(user);
        context.Folders.Add(folder);
        await context.SaveChangesAsync();

        var service = new PermissionService(context);

        // Act
        var result = await service.CanAccessFolderAsync(user.Id, folder.Id, "Read");

        // Assert
        result.Should().BeTrue();

        DbContextHelper.CleanupDbContext(context);
    }

    [Fact]
    public async Task CanAccessFolderAsync_WithPermission_ReturnsTrue()
    {
        // Arrange
        var context = DbContextHelper.GetInMemoryDbContext();
        var owner = EdmFixtures.GetTestUser();
        var otherUser = EdmFixtures.GetTestUser();
        otherUser.Id = Guid.NewGuid();
        otherUser.Username = "otheruser";
        otherUser.Email = "other@example.com";
        
        var folder = EdmFixtures.GetTestFolder(owner.Id);
        var permission = EdmFixtures.GetTestPermission(otherUser.Id, folderId: folder.Id);
        permission.PermissionType = "Read";
        
        context.Users.AddRange(owner, otherUser);
        context.Folders.Add(folder);
        context.Permissions.Add(permission);
        await context.SaveChangesAsync();

        var service = new PermissionService(context);

        // Act
        var result = await service.CanAccessFolderAsync(otherUser.Id, folder.Id, "Read");

        // Assert
        result.Should().BeTrue();

        DbContextHelper.CleanupDbContext(context);
    }

    [Fact]
    public async Task CanAccessFolderAsync_WithoutPermission_ReturnsFalse()
    {
        // Arrange
        var context = DbContextHelper.GetInMemoryDbContext();
        var owner = EdmFixtures.GetTestUser();
        var otherUser = EdmFixtures.GetTestUser();
        otherUser.Id = Guid.NewGuid();
        otherUser.Username = "otheruser";
        otherUser.Email = "other@example.com";
        
        var folder = EdmFixtures.GetTestFolder(owner.Id);
        
        context.Users.AddRange(owner, otherUser);
        context.Folders.Add(folder);
        await context.SaveChangesAsync();

        var service = new PermissionService(context);

        // Act
        var result = await service.CanAccessFolderAsync(otherUser.Id, folder.Id, "Read");

        // Assert
        result.Should().BeFalse();

        DbContextHelper.CleanupDbContext(context);
    }

    [Fact]
    public async Task CanAccessFolderAsync_SystemFolder_Read_ReturnsTrueForAnyUser()
    {
        // Arrange
        var context = DbContextHelper.GetInMemoryDbContext();
        var user = EdmFixtures.GetTestUser();
        var admin = EdmFixtures.GetTestSystemAdmin();
        var folder = EdmFixtures.GetTestFolder(admin.Id);
        folder.IsSystemFolder = true;
        
        context.Users.AddRange(user, admin);
        context.Folders.Add(folder);
        await context.SaveChangesAsync();

        var service = new PermissionService(context);

        // Act
        var result = await service.CanAccessFolderAsync(user.Id, folder.Id, "Read");

        // Assert
        result.Should().BeTrue();

        DbContextHelper.CleanupDbContext(context);
    }

    [Fact]
    public async Task CanAccessFolderAsync_SystemFolder_Write_ReturnsTrueForEditor()
    {
        // Arrange
        var context = DbContextHelper.GetInMemoryDbContext();
        var editor = EdmFixtures.GetTestUser("Editor");
        var admin = EdmFixtures.GetTestSystemAdmin();
        var folder = EdmFixtures.GetTestFolder(admin.Id);
        folder.IsSystemFolder = true;
        
        context.Users.AddRange(editor, admin);
        context.Folders.Add(folder);
        await context.SaveChangesAsync();

        var service = new PermissionService(context);

        // Act
        var result = await service.CanAccessFolderAsync(editor.Id, folder.Id, "Write");

        // Assert
        result.Should().BeTrue();

        DbContextHelper.CleanupDbContext(context);
    }

    [Fact]
    public async Task CanAccessFolderAsync_SystemFolder_Write_ReturnsFalseForViewer()
    {
        // Arrange
        var context = DbContextHelper.GetInMemoryDbContext();
        var viewer = EdmFixtures.GetTestUser("Viewer");
        var admin = EdmFixtures.GetTestSystemAdmin();
        var folder = EdmFixtures.GetTestFolder(admin.Id);
        folder.IsSystemFolder = true;
        
        context.Users.AddRange(viewer, admin);
        context.Folders.Add(folder);
        await context.SaveChangesAsync();

        var service = new PermissionService(context);

        // Act
        var result = await service.CanAccessFolderAsync(viewer.Id, folder.Id, "Write");

        // Assert
        result.Should().BeFalse();

        DbContextHelper.CleanupDbContext(context);
    }

    [Fact]
    public async Task CanAccessDocumentAsync_Owner_ReturnsTrue()
    {
        // Arrange
        var context = DbContextHelper.GetInMemoryDbContext();
        var user = EdmFixtures.GetTestUser();
        var folder = EdmFixtures.GetTestFolder(user.Id);
        var document = EdmFixtures.GetTestDocument(folder.Id, user.Id);
        
        context.Users.Add(user);
        context.Folders.Add(folder);
        context.Documents.Add(document);
        await context.SaveChangesAsync();

        var service = new PermissionService(context);

        // Act
        var result = await service.CanAccessDocumentAsync(user.Id, document.Id, "Read");

        // Assert
        result.Should().BeTrue();

        DbContextHelper.CleanupDbContext(context);
    }

    [Fact]
    public async Task CanAccessDocumentAsync_WithDocumentPermission_ReturnsTrue()
    {
        // Arrange
        var context = DbContextHelper.GetInMemoryDbContext();
        var owner = EdmFixtures.GetTestUser();
        var otherUser = EdmFixtures.GetTestUser();
        otherUser.Id = Guid.NewGuid();
        otherUser.Username = "otheruser";
        otherUser.Email = "other@example.com";
        
        var folder = EdmFixtures.GetTestFolder(owner.Id);
        var document = EdmFixtures.GetTestDocument(folder.Id, owner.Id);
        var permission = EdmFixtures.GetTestPermission(otherUser.Id, documentId: document.Id);
        permission.PermissionType = "Read";
        
        context.Users.AddRange(owner, otherUser);
        context.Folders.Add(folder);
        context.Documents.Add(document);
        context.Permissions.Add(permission);
        await context.SaveChangesAsync();

        var service = new PermissionService(context);

        // Act
        var result = await service.CanAccessDocumentAsync(otherUser.Id, document.Id, "Read");

        // Assert
        result.Should().BeTrue();

        DbContextHelper.CleanupDbContext(context);
    }

    [Fact]
    public async Task CanAccessDocumentAsync_WithInheritedFolderPermission_ReturnsTrue()
    {
        // Arrange
        var context = DbContextHelper.GetInMemoryDbContext();
        var owner = EdmFixtures.GetTestUser();
        var otherUser = EdmFixtures.GetTestUser();
        otherUser.Id = Guid.NewGuid();
        otherUser.Username = "otheruser";
        otherUser.Email = "other@example.com";
        
        var folder = EdmFixtures.GetTestFolder(owner.Id);
        var document = EdmFixtures.GetTestDocument(folder.Id, owner.Id);
        var folderPermission = EdmFixtures.GetTestPermission(otherUser.Id, folderId: folder.Id);
        folderPermission.PermissionType = "Read";
        
        context.Users.AddRange(owner, otherUser);
        context.Folders.Add(folder);
        context.Documents.Add(document);
        context.Permissions.Add(folderPermission);
        await context.SaveChangesAsync();

        var service = new PermissionService(context);

        // Act
        var result = await service.CanAccessDocumentAsync(otherUser.Id, document.Id, "Read");

        // Assert
        result.Should().BeTrue();

        DbContextHelper.CleanupDbContext(context);
    }

    [Fact]
    public async Task GrantPermissionAsync_CreatesPermission()
    {
        // Arrange
        var context = DbContextHelper.GetInMemoryDbContext();
        var owner = EdmFixtures.GetTestUser();
        var otherUser = EdmFixtures.GetTestUser();
        otherUser.Id = Guid.NewGuid();
        otherUser.Username = "otheruser";
        otherUser.Email = "other@example.com";
        
        var folder = EdmFixtures.GetTestFolder(owner.Id);
        
        context.Users.AddRange(owner, otherUser);
        context.Folders.Add(folder);
        await context.SaveChangesAsync();

        var service = new PermissionService(context);

        // Act
        var request = new Backend.Models.DTO.Permissions.CreatePermissionRequest
        {
            UserId = otherUser.Id,
            FolderId = folder.Id,
            PermissionType = "Read"
        };
        var result = await service.GrantPermissionAsync(request, owner.Username);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(otherUser.Id);
        result.FolderId.Should().Be(folder.Id);
        result.PermissionType.Should().Be("Read");

        DbContextHelper.CleanupDbContext(context);
    }

    [Fact]
    public async Task RevokePermissionAsync_RemovesPermission()
    {
        // Arrange
        var context = DbContextHelper.GetInMemoryDbContext();
        var owner = EdmFixtures.GetTestUser();
        var otherUser = EdmFixtures.GetTestUser();
        otherUser.Id = Guid.NewGuid();
        otherUser.Username = "otheruser";
        otherUser.Email = "other@example.com";
        
        var folder = EdmFixtures.GetTestFolder(owner.Id);
        var permission = EdmFixtures.GetTestPermission(otherUser.Id, folderId: folder.Id);
        
        context.Users.AddRange(owner, otherUser);
        context.Folders.Add(folder);
        context.Permissions.Add(permission);
        await context.SaveChangesAsync();

        var service = new PermissionService(context);

        // Act
        var result = await service.RevokePermissionAsync(permission.Id);

        // Assert
        result.Should().BeTrue();
        var deletedPermission = await context.Permissions.FindAsync(permission.Id);
        deletedPermission.Should().BeNull();

        DbContextHelper.CleanupDbContext(context);
    }

    [Fact]
    public async Task GetUserPermissionsAsync_ReturnsUserPermissions()
    {
        // Arrange
        var context = DbContextHelper.GetInMemoryDbContext();
        var owner = EdmFixtures.GetTestUser();
        var otherUser = EdmFixtures.GetTestUser();
        otherUser.Id = Guid.NewGuid();
        otherUser.Username = "otheruser";
        otherUser.Email = "other@example.com";
        
        var folder1 = EdmFixtures.GetTestFolder(owner.Id);
        var folder2 = EdmFixtures.GetTestFolder(owner.Id);
        folder2.Id = Guid.NewGuid();
        folder2.Name = "Folder 2";
        
        var permission1 = EdmFixtures.GetTestPermission(otherUser.Id, folderId: folder1.Id);
        var permission2 = EdmFixtures.GetTestPermission(otherUser.Id, folderId: folder2.Id);
        permission2.Id = Guid.NewGuid();
        
        context.Users.AddRange(owner, otherUser);
        context.Folders.AddRange(folder1, folder2);
        context.Permissions.AddRange(permission1, permission2);
        await context.SaveChangesAsync();

        var service = new PermissionService(context);

        // Act
        var result = await service.GetUserPermissionsAsync(otherUser.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        DbContextHelper.CleanupDbContext(context);
    }
}
