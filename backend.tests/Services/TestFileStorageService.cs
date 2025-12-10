using Backend.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace backend.tests.Services;

/// <summary>
/// Tests for FirebaseFileStorageService
/// Note: These tests require Firebase Storage to be configured
/// Set Firebase credentials in test environment or skip with [Skip] attribute
/// </summary>
public class TestFileStorageService
{
    private readonly IConfiguration _configuration;

    public TestFileStorageService()
    {
        var inMemorySettings = new Dictionary<string, string>
        {
            {"FileStorage:BasePath", Path.Combine(Path.GetTempPath(), "test-edm-files")},
            {"FileStorage:MaxFileSizeMB", "10"},
            {"FileStorage:AllowedExtensions", ".pdf,.doc,.docx,.txt,.jpg,.png"},
            // Firebase settings - configure for testing
            {"Firebase:StorageBucket", "test-bucket.appspot.com"},
            {"Firebase:CredentialsPath", ""},
            {"Firebase:CredentialsJson", ""}
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();
    }

    [Fact]
    public async Task SaveFileAsync_WithValidFile_SavesSuccessfully()
    {
        // Skip test if Firebase is not configured
        var bucket = _configuration["Firebase:StorageBucket"];
        if (string.IsNullOrEmpty(bucket) || bucket == "test-bucket.appspot.com")
        {
            return; // Skip the test
        }

        // Arrange
        var service = new FirebaseFileStorageService(_configuration);
        var fileName = "test-document.pdf";
        var content = new byte[] { 1, 2, 3, 4, 5 };
        var stream = new MemoryStream(content);
        var documentId = Guid.NewGuid();

        try
        {
            // Act
            var result = await service.SaveFileAsync(stream, fileName, documentId, 1);

            // Assert
            result.Should().NotBeNullOrEmpty();
            result.Should().Contain("documents/");
            result.Should().Contain(documentId.ToString());
            result.Should().Contain("v1_test-document.pdf");

            // Cleanup - delete from Firebase
            await service.DeleteFileAsync(result);
        }
        finally
        {
            stream.Dispose();
        }
    }

    [Fact]
    public async Task GetFileAsync_WithExistingFile_ReturnsStream()
    {
        // Arrange
        var service = new FileStorageService(_configuration);
        var fileName = "test-document.pdf";
        var content = new byte[] { 1, 2, 3, 4, 5 };
        var writeStream = new MemoryStream(content);
        var documentId = Guid.NewGuid();

        try
        {
            var filePath = await service.SaveFileAsync(writeStream, fileName, documentId, 1);

            // Act
            var readStream = await service.GetFileAsync(filePath);

            // Assert
            readStream.Should().NotBeNull();
            var readContent = new byte[content.Length];
            await readStream!.ReadAsync(readContent, 0, readContent.Length);
            readContent.Should().BeEquivalentTo(content);

            // Cleanup
            readStream.Dispose();
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        finally
        {
            writeStream.Dispose();
        }
    }

    [Fact]
    public async Task GetFileAsync_WithNonExistentFile_ReturnsNull()
    {
        // Arrange
        var service = new FileStorageService(_configuration);
        var nonExistentPath = Path.Combine(Path.GetTempPath(), "nonexistent.pdf");

        // Act
        var result = await service.GetFileAsync(nonExistentPath);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteFileAsync_WithExistingFile_DeletesSuccessfully()
    {
        // Arrange
        var service = new FileStorageService(_configuration);
        var fileName = "test-document.pdf";
        var content = new byte[] { 1, 2, 3, 4, 5 };
        var stream = new MemoryStream(content);
        var documentId = Guid.NewGuid();

        try
        {
            var relativePath = await service.SaveFileAsync(stream, fileName, documentId, 1);
            var basePath = Path.Combine(Path.GetTempPath(), "test-edm-files");
            var fullPath = Path.Combine(basePath, relativePath);
            File.Exists(fullPath).Should().BeTrue();

            // Act
            await service.DeleteFileAsync(relativePath);

            // Assert
            File.Exists(fullPath).Should().BeFalse();
        }
        finally
        {
            stream.Dispose();
        }
    }

    [Fact]
    public async Task FileExistsAsync_WithExistingFile_ReturnsTrue()
    {
        // Arrange
        var service = new FileStorageService(_configuration);
        var fileName = "test-document.pdf";
        var content = new byte[] { 1, 2, 3, 4, 5 };
        var stream = new MemoryStream(content);
        var documentId = Guid.NewGuid();

        try
        {
            var filePath = await service.SaveFileAsync(stream, fileName, documentId, 1);

            // Act
            var result = await service.FileExistsAsync(filePath);

            // Assert
            result.Should().BeTrue();

            // Cleanup
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        finally
        {
            stream.Dispose();
        }
    }

    [Fact]
    public void IsAllowedExtension_WithAllowedExtension_ReturnsTrue()
    {
        // Arrange
        var service = new FileStorageService(_configuration);

        // Act
        var result = service.IsAllowedExtension(".pdf");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsAllowedExtension_WithDisallowedExtension_ReturnsFalse()
    {
        // Arrange
        var service = new FileStorageService(_configuration);

        // Act
        var result = service.IsAllowedExtension(".exe");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsFileSizeValid_WithValidSize_ReturnsTrue()
    {
        // Arrange
        var service = new FileStorageService(_configuration);
        var sizeInBytes = 5 * 1024 * 1024; // 5 MB

        // Act
        var result = service.IsFileSizeValid(sizeInBytes);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsFileSizeValid_WithInvalidSize_ReturnsFalse()
    {
        // Arrange
        var service = new FileStorageService(_configuration);
        var sizeInBytes = 15 * 1024 * 1024; // 15 MB (max is 10)

        // Act
        var result = service.IsFileSizeValid(sizeInBytes);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetFileExtension_ReturnsCorrectExtension()
    {
        // Arrange
        var service = new FileStorageService(_configuration);

        // Act
        var result = service.GetFileExtension("document.pdf");

        // Assert
        result.Should().Be(".pdf");
    }
}
