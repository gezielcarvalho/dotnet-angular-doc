using Backend.Models.Document;
using Backend.Models.DTO.Auth;

namespace backend.tests.Fixtures;

public static class EdmFixtures
{
    public static User GetTestUser(string role = "User")
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@123"),
            FirstName = "Test",
            LastName = "User",
            Role = role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System"
        };
    }

    public static User GetTestAdmin()
    {
        return GetTestUser("Admin");
    }

    public static User GetTestSystemAdmin()
    {
        return GetTestUser("SystemAdmin");
    }

    public static Folder GetTestFolder(Guid ownerId, Guid? parentFolderId = null)
    {
        return new Folder
        {
            Id = Guid.NewGuid(),
            Name = "Test Folder",
            Description = "Test folder description",
            ParentFolderId = parentFolderId,
            Path = parentFolderId == null ? "/Test Folder" : "/Parent/Test Folder",
            Level = parentFolderId == null ? 0 : 1,
            IsSystemFolder = false,
            OwnerId = ownerId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System"
        };
    }

    public static Document GetTestDocument(Guid folderId, Guid ownerId)
    {
        return new Document
        {
            Id = Guid.NewGuid(),
            Title = "Test Document",
            Description = "Test document description",
            FolderId = folderId,
            FileName = "test.pdf",
            FileSize = 1024,
            MimeType = "application/pdf",
            FileExtension = ".pdf",
            CurrentVersion = 1,
            Status = "Active",
            OwnerId = ownerId,
            IsPublic = false,
            ViewCount = 0,
            DownloadCount = 0,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System"
        };
    }

    public static Tag GetTestTag()
    {
        return new Tag
        {
            Id = Guid.NewGuid(),
            Name = "Test Tag",
            Description = "Test tag description",
            Color = "#FF0000",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System"
        };
    }

    public static Permission GetTestPermission(Guid userId, Guid? folderId = null, Guid? documentId = null)
    {
        return new Permission
        {
            Id = Guid.NewGuid(),
            FolderId = folderId,
            DocumentId = documentId,
            UserId = userId,
            PermissionType = "Read",
            IsInherited = false,
            GrantedAt = DateTime.UtcNow,
            GrantedBy = "System",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System"
        };
    }

    public static LoginRequest GetLoginRequest()
    {
        return new LoginRequest
        {
            Username = "testuser",
            Password = "Test@123"
        };
    }

    public static RegisterRequest GetRegisterRequest()
    {
        return new RegisterRequest
        {
            Username = "newuser",
            Email = "newuser@example.com",
            Password = "NewUser@123",
            FirstName = "New",
            LastName = "User",
            Department = "IT"
        };
    }
}
