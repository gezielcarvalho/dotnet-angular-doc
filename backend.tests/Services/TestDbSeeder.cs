using backend.tests.Fixtures;
using backend.tests.Helpers;
using Backend.Data;
using Backend.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace backend.tests.Services;

public class TestDbSeeder
{
    [Fact]
    public async Task SeedAsync_ExistingUsers_CreatesPersonalFolders()
    {
        // Arrange: create context with existing users but no folders
        var context = DbContextHelper.GetInMemoryDbContext();
        var user1 = EdmFixtures.GetTestUser();
        var user2 = EdmFixtures.GetTestUser();
        user2.Id = Guid.NewGuid();
        user2.Username = "otheruser";
        user2.Email = "other@example.com";

        context.Users.AddRange(user1, user2);
        await context.SaveChangesAsync();

        // Act
        await DbSeeder.SeedAsync(context);

        // Assert: Users folder exists and personal folders exist for user1 and user2
        var root = await context.Folders.FirstOrDefaultAsync(f => f.ParentFolderId == null);
        root.Should().NotBeNull();
        var usersParent = await context.Folders.FirstOrDefaultAsync(f => f.Name == "Users" && f.ParentFolderId == root!.Id);
        usersParent.Should().NotBeNull();

        var personal1 = await context.Folders.FirstOrDefaultAsync(f => f.ParentFolderId == usersParent!.Id && f.OwnerId == user1.Id);
        var personal2 = await context.Folders.FirstOrDefaultAsync(f => f.ParentFolderId == usersParent!.Id && f.OwnerId == user2.Id);
        personal1.Should().NotBeNull();
        personal2.Should().NotBeNull();

        // Each personal folder should have an Admin permission for its user
        var perm1 = await context.Permissions.FirstOrDefaultAsync(p => p.FolderId == personal1!.Id && p.UserId == user1.Id && p.PermissionType == "Admin");
        var perm2 = await context.Permissions.FirstOrDefaultAsync(p => p.FolderId == personal2!.Id && p.UserId == user2.Id && p.PermissionType == "Admin");
        perm1.Should().NotBeNull();
        perm2.Should().NotBeNull();

        DbContextHelper.CleanupDbContext(context);
    }
}
