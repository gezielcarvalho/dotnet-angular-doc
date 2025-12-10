using Backend.Services.Interfaces;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using System.Text.Json;

namespace Backend.Services;

public class FirebaseFileStorageService : IFileStorageService
{
    private readonly StorageClient _storageClient;
    private readonly string _bucketName;
    private readonly long _maxFileSizeMB;
    private readonly List<string> _allowedExtensions;

    public FirebaseFileStorageService(IConfiguration configuration)
    {
        var fileStorageConfig = configuration.GetSection("FileStorage");
        _maxFileSizeMB = long.Parse(fileStorageConfig["MaxFileSizeMB"] ?? "100");
        _allowedExtensions = fileStorageConfig.GetSection("AllowedExtensions")
            .Get<List<string>>() ?? new List<string> { ".pdf", ".doc", ".docx" };

        // Get Firebase configuration
        var firebaseConfig = configuration.GetSection("Firebase");
        _bucketName = firebaseConfig["StorageBucket"] ?? throw new InvalidOperationException("Firebase StorageBucket not configured");

        // Initialize Firebase Admin SDK
        var credentialsPath = firebaseConfig["CredentialsPath"];
        var credentialsJson = firebaseConfig["CredentialsJson"];

        if (!string.IsNullOrEmpty(credentialsPath) && File.Exists(credentialsPath))
        {
            // Use service account file
            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile(credentialsPath)
            });
        }
        else if (!string.IsNullOrEmpty(credentialsJson))
        {
            // Use JSON string (for environment variables)
            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromJson(credentialsJson)
            });
        }
        else
        {
            // Use default credentials (for GCP environments)
            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.GetApplicationDefault()
            });
        }

        _storageClient = StorageClient.Create();
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, Guid documentId, int version)
    {
        var extension = GetFileExtension(fileName);
        if (!IsAllowedExtension(extension))
            throw new InvalidOperationException($"File extension {extension} is not allowed");

        // Create object name: documents/{documentId}/v{version}_{fileName}
        var objectName = $"documents/{documentId}/v{version}_{fileName}";

        // Upload to Firebase Storage
        var uploadObject = await _storageClient.UploadObjectAsync(
            _bucketName,
            objectName,
            null, // contentType - let Firebase detect it
            fileStream
        );

        // Return the object name for storage in database
        return objectName;
    }

    public async Task<Stream?> GetFileAsync(string filePath)
    {
        try
        {
            var memoryStream = new MemoryStream();
            await _storageClient.DownloadObjectAsync(_bucketName, filePath, memoryStream);
            memoryStream.Position = 0;
            return memoryStream;
        }
        catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<bool> DeleteFileAsync(string filePath)
    {
        try
        {
            await _storageClient.DeleteObjectAsync(_bucketName, filePath);
            return true;
        }
        catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> FileExistsAsync(string filePath)
    {
        try
        {
            await _storageClient.GetObjectAsync(_bucketName, filePath);
            return true;
        }
        catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<long> GetFileSizeAsync(string filePath)
    {
        try
        {
            var obj = await _storageClient.GetObjectAsync(_bucketName, filePath);
            return (long)(obj.Size ?? 0);
        }
        catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return 0;
        }
        catch
        {
            return 0;
        }
    }

    public string GetFileExtension(string fileName)
    {
        return Path.GetExtension(fileName).ToLowerInvariant();
    }

    public bool IsAllowedExtension(string extension)
    {
        return _allowedExtensions.Contains(extension.ToLowerInvariant());
    }

    public bool IsFileSizeValid(long fileSizeBytes)
    {
        var maxBytes = _maxFileSizeMB * 1024 * 1024;
        return fileSizeBytes <= maxBytes;
    }
}