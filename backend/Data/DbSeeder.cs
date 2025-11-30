using Backend.Models.Document;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(DocumentDbContext context)
    {
        // Check if we already have data
        if (await context.Users.AnyAsync())
            return;
        // If no users exist, seed initial data
        if (!await context.Users.AnyAsync())
            {
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

                // Create 'Users' system folder to host per-user folders
                var usersFolder = new Folder
                {
                    Id = Guid.NewGuid(),
                    Name = "Users",
                    Description = "Personal folders for users",
                    Path = "/Root/Users/",
                    Level = 1,
                    ParentFolderId = rootFolder.Id,
                    IsSystemFolder = true,
                    OwnerId = adminUser.Id,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                };
                context.Folders.Add(usersFolder);

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

                // Create personal folder for admin user under Users
                var adminPersonalFolder = new Folder
                {
                    Id = Guid.NewGuid(),
                    Name = adminUser.Username,
                    Description = "Personal folder for admin user",
                    Path = $"{usersFolder.Path}{adminUser.Username}/",
                    Level = usersFolder.Level + 1,
                    ParentFolderId = usersFolder.Id,
                    IsSystemFolder = false,
                    OwnerId = adminUser.Id,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                };
                context.Folders.Add(adminPersonalFolder);
                await context.SaveChangesAsync();

                var adminPermission = new Permission
                {
                    Id = Guid.NewGuid(),
                    UserId = adminUser.Id,
                    FolderId = adminPersonalFolder.Id,
                    PermissionType = "Admin",
                    IsInherited = false,
                    GrantedAt = DateTime.UtcNow,
                    GrantedBy = "System",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                };
                context.Permissions.Add(adminPermission);
                await context.SaveChangesAsync();
            }

            // Ensure personal folders exist for all existing users
            await EnsurePersonalFoldersForExistingUsers(context);
        }

        public static async Task<int> EnsurePersonalFoldersForExistingUsers(DocumentDbContext context)
        {
            // Ensure root exists
            var root = await context.Folders.FirstOrDefaultAsync(f => f.ParentFolderId == null);
            if (root == null)
            {
                root = new Folder
                {
                    Id = Guid.NewGuid(),
                    Name = "Root",
                    Description = "Root folder",
                    Path = "/Root/",
                    Level = 0,
                    ParentFolderId = null,
                    IsSystemFolder = true,
                    OwnerId = (await context.Users.FirstOrDefaultAsync(u => u.Role == "SystemAdmin" || u.Role == "Admin"))?.Id ?? Guid.Empty,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                };
                context.Folders.Add(root);
                await context.SaveChangesAsync();
            }

            // Ensure Users system folder exists
            var usersFolder = await context.Folders.FirstOrDefaultAsync(f => f.Name == "Users" && f.ParentFolderId == root.Id && f.IsSystemFolder);
            if (usersFolder == null)
            {
                usersFolder = new Folder
                {
                    Id = Guid.NewGuid(),
                    Name = "Users",
                    Description = "Personal folders for users",
                    Path = $"{root.Path}Users/",
                    Level = root.Level + 1,
                    ParentFolderId = root.Id,
                    IsSystemFolder = true,
                    OwnerId = Guid.Empty,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                };
                context.Folders.Add(usersFolder);
                await context.SaveChangesAsync();
            }

            // Create personal folders for all users that don't have one
            var users = await context.Users.Where(u => !u.IsDeleted).ToListAsync();
            var createdCount = 0;
            foreach (var user in users)
            {
                var personal = await context.Folders.FirstOrDefaultAsync(f => f.ParentFolderId == usersFolder.Id && f.OwnerId == user.Id);
                if (personal == null)
                {
                    personal = new Folder
                    {
                        Id = Guid.NewGuid(),
                        Name = user.Username,
                        Description = $"Personal folder for {user.Username}",
                        Path = $"{usersFolder.Path}{user.Username}/",
                        Level = usersFolder.Level + 1,
                        ParentFolderId = usersFolder.Id,
                        IsSystemFolder = false,
                        OwnerId = user.Id,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "System"
                    };
                    context.Folders.Add(personal);
                    await context.SaveChangesAsync();

                    // Grant explicit admin permission for the user's personal folder
                    var permission = new Permission
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        FolderId = personal.Id,
                        PermissionType = "Admin",
                        IsInherited = false,
                        GrantedAt = DateTime.UtcNow,
                        GrantedBy = "System",
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "System"
                    };
                    context.Permissions.Add(permission);
                    await context.SaveChangesAsync();
                    createdCount++;
                }
            }
            return createdCount;
        }
}
