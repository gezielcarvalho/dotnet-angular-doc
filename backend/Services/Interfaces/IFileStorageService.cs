namespace Backend.Services.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(Stream fileStream, string fileName, Guid documentId, int version);
    Task<Stream?> GetFileAsync(string filePath);
    Task<bool> DeleteFileAsync(string filePath);
    Task<bool> FileExistsAsync(string filePath);
    Task<long> GetFileSizeAsync(string filePath);
    string GetFileExtension(string fileName);
    bool IsAllowedExtension(string extension);
    bool IsFileSizeValid(long fileSizeBytes);
}
