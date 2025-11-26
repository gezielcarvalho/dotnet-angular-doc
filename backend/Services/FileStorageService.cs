using Backend.Services.Interfaces;

namespace Backend.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IConfiguration _configuration;
    private readonly string _basePath;
    private readonly long _maxFileSizeMB;
    private readonly List<string> _allowedExtensions;

    public FileStorageService(IConfiguration configuration)
    {
        _configuration = configuration;
        
        var fileStorageConfig = _configuration.GetSection("FileStorage");
        _basePath = fileStorageConfig["BasePath"] ?? "FileStorage";
        _maxFileSizeMB = long.Parse(fileStorageConfig["MaxFileSizeMB"] ?? "100");
        _allowedExtensions = fileStorageConfig.GetSection("AllowedExtensions")
            .Get<List<string>>() ?? new List<string> { ".pdf", ".doc", ".docx" };

        // Ensure base path exists
        if (!Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
        }
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, Guid documentId, int version)
    {
        var extension = GetFileExtension(fileName);
        if (!IsAllowedExtension(extension))
            throw new InvalidOperationException($"File extension {extension} is not allowed");

        // Create directory structure: FileStorage/DocumentId/
        var documentDir = Path.Combine(_basePath, documentId.ToString());
        if (!Directory.Exists(documentDir))
        {
            Directory.CreateDirectory(documentDir);
        }

        // File naming: v{version}_{originalFileName}
        var storedFileName = $"v{version}_{fileName}";
        var filePath = Path.Combine(documentDir, storedFileName);

        // Save file
        using var fileStreamOutput = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        await fileStream.CopyToAsync(fileStreamOutput);

        // Return relative path for storage in database
        return Path.Combine(documentId.ToString(), storedFileName);
    }

    public async Task<Stream?> GetFileAsync(string filePath)
    {
        var fullPath = Path.Combine(_basePath, filePath);
        
        if (!File.Exists(fullPath))
            return null;

        var memoryStream = new MemoryStream();
        using var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
        await fileStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;
        
        return memoryStream;
    }

    public Task<bool> DeleteFileAsync(string filePath)
    {
        try
        {
            var fullPath = Path.Combine(_basePath, filePath);
            
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                return Task.FromResult(true);
            }
            
            return Task.FromResult(false);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public Task<bool> FileExistsAsync(string filePath)
    {
        var fullPath = Path.Combine(_basePath, filePath);
        return Task.FromResult(File.Exists(fullPath));
    }

    public Task<long> GetFileSizeAsync(string filePath)
    {
        var fullPath = Path.Combine(_basePath, filePath);
        
        if (!File.Exists(fullPath))
            return Task.FromResult(0L);

        var fileInfo = new FileInfo(fullPath);
        return Task.FromResult(fileInfo.Length);
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
