using System.IO;

namespace Backend.Services;

/// <summary>
/// Utility class for file storage operations that can be tested independently.
/// </summary>
public static class FileStorageUtils
{
    /// <summary>
    /// Gets the file extension from a file name in lowercase.
    /// </summary>
    public static string GetFileExtension(string fileName)
    {
        return Path.GetExtension(fileName).ToLowerInvariant();
    }

    /// <summary>
    /// Checks if a file extension is in the list of allowed extensions.
    /// </summary>
    public static bool IsAllowedExtension(string extension, List<string> allowedExtensions)
    {
        return allowedExtensions.Contains(extension.ToLowerInvariant());
    }

    /// <summary>
    /// Checks if a file size is within the allowed limit.
    /// </summary>
    public static bool IsFileSizeValid(long fileSizeBytes, long maxFileSizeMB)
    {
        var maxBytes = maxFileSizeMB * 1024 * 1024;
        return fileSizeBytes <= maxBytes;
    }
}