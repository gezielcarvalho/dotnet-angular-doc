using Backend.Models.Document;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(EdmDbContext context)
    {
        // Check if we already have data
        if (await context.Users.AnyAsync())
            return;

        // Create a system admin user
        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            Username = "admin",
            Email = "admin@edm.com",
            FirstName = "System",
            LastName = "Administrator",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            Role = "SystemAdmin",
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System"
        };

        context.Users.Add(adminUser);

        // Create root folder
        var rootFolder = new Folder
        {
            Id = Guid.NewGuid(),
            Name = "Root",
            Description = "Root folder for all documents",
            Path = "/Root/",
            Level = 0,
            ParentFolderId = null,
            IsSystemFolder = true,
            OwnerId = adminUser.Id,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System"
        };

        context.Folders.Add(rootFolder);

        // Create some default folders
        var folders = new List<Folder>
        {
            new Folder
            {
                Id = Guid.NewGuid(),
                Name = "General",
                Description = "General documents",
                Path = "/Root/General/",
                Level = 1,
                ParentFolderId = rootFolder.Id,
                IsSystemFolder = false,
                OwnerId = adminUser.Id,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new Folder
            {
                Id = Guid.NewGuid(),
                Name = "Projects",
                Description = "Project documents",
                Path = "/Root/Projects/",
                Level = 1,
                ParentFolderId = rootFolder.Id,
                IsSystemFolder = false,
                OwnerId = adminUser.Id,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new Folder
            {
                Id = Guid.NewGuid(),
                Name = "Archive",
                Description = "Archived documents",
                Path = "/Root/Archive/",
                Level = 1,
                ParentFolderId = rootFolder.Id,
                IsSystemFolder = false,
                OwnerId = adminUser.Id,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            }
        };

        context.Folders.AddRange(folders);

        // Create some default tags
        var tags = new List<Tag>
        {
            new Tag
            {
                Id = Guid.NewGuid(),
                Name = "Important",
                Description = "Important documents",
                Color = "#EF4444",
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new Tag
            {
                Id = Guid.NewGuid(),
                Name = "Draft",
                Description = "Draft documents",
                Color = "#F59E0B",
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new Tag
            {
                Id = Guid.NewGuid(),
                Name = "Final",
                Description = "Final documents",
                Color = "#10B981",
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            }
        };

        context.Tags.AddRange(tags);

        await context.SaveChangesAsync();
    }
}
