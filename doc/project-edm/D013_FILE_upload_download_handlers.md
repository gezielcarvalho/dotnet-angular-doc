# EDM System - File Upload/Download Handlers

## Overview

This document defines the complete file storage infrastructure for the EDM system, including file upload handlers, download streaming, storage organization, virus scanning, and file type validation.

## File Storage Architecture

```
┌─────────────────────────────────────────────────────────────┐
│              File Storage Architecture                       │
│                                                              │
│  HTTP Request (Multipart)                                   │
│         ↓                                                    │
│  ┌──────────────────────────────────────────────────┐      │
│  │         Upload Middleware                         │      │
│  │  • File size validation                           │      │
│  │  • MIME type validation                           │      │
│  │  • Virus scanning (optional)                      │      │
│  └──────────────────────────────────────────────────┘      │
│         ↓                                                    │
│  ┌──────────────────────────────────────────────────┐      │
│  │         FileStorageService                        │      │
│  │  • Generate unique file path                      │      │
│  │  • Save to disk                                   │      │
│  │  • Create thumbnails (images/PDFs)                │      │
│  │  • Extract metadata                               │      │
│  └──────────────────────────────────────────────────┘      │
│         ↓                                                    │
│  ┌──────────────────────────────────────────────────┐      │
│  │         File System Storage                       │      │
│  │  /EDMStorage/                                     │      │
│  │    ├── Documents/                                 │      │
│  │    │   ├── {year}/                                │      │
│  │    │   │   ├── {month}/                           │      │
│  │    │   │   │   ├── {documentId}/                  │      │
│  │    │   │   │   │   ├── v1_originalname.ext        │      │
│  │    │   │   │   │   ├── v2_originalname.ext        │      │
│  │    ├── Thumbnails/                                │      │
│  │    ├── Temp/                                      │      │
│  └──────────────────────────────────────────────────┘      │
└─────────────────────────────────────────────────────────────┘
```

## File Storage Service

### IFileStorageService Interface

```csharp
namespace Backend.Services.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(IFormFile file, Guid documentId, int version);
        Task<Stream> GetFileAsync(string filePath);
        Task<Stream> GetFileVersionAsync(Guid documentId, int version);
        Task DeleteFileAsync(string filePath);
        Task DeleteDocumentFilesAsync(Guid documentId);
        Task<bool> FileExistsAsync(string filePath);
        Task<long> GetFileSizeAsync(string filePath);
        Task<string> GenerateThumbnailAsync(string filePath, Guid documentId);
        Task<Dictionary<string, string>> ExtractMetadataAsync(IFormFile file);
        string GetStorageBasePath();
        Task<long> GetTotalStorageUsedAsync();
    }
}
```

### FileStorageService Implementation

```csharp
using Microsoft.Extensions.Options;
using Backend.Services.Interfaces;
using Backend.Models.Configuration;
using System.IO;
using System.Security.Cryptography;

namespace Backend.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly FileStorageSettings _settings;
        private readonly ILogger<FileStorageService> _logger;

        public FileStorageService(
            IOptions<FileStorageSettings> settings,
            ILogger<FileStorageService> logger)
        {
            _settings = settings.Value;
            _logger = logger;

            // Ensure base directory exists
            EnsureDirectoryExists(_settings.BasePath);
        }

        public async Task<string> SaveFileAsync(IFormFile file, Guid documentId, int version)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is required");

            // Validate file size
            if (file.Length > _settings.MaxFileSizeBytes)
                throw new ArgumentException($"File size exceeds maximum limit of {_settings.MaxFileSizeBytes / 1048576} MB");

            // Generate file path
            var filePath = GenerateFilePath(documentId, version, file.FileName);
            var fullPath = Path.Combine(_settings.BasePath, filePath);

            // Ensure directory exists
            var directory = Path.GetDirectoryName(fullPath);
            EnsureDirectoryExists(directory);

            try
            {
                // Save file
                using (var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await file.CopyToAsync(stream);
                }

                _logger.LogInformation("File saved: {FilePath}", filePath);

                // Generate thumbnail for images/PDFs
                if (IsImageFile(file.FileName) || IsPdfFile(file.FileName))
                {
                    await GenerateThumbnailAsync(filePath, documentId);
                }

                return filePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving file: {FilePath}", filePath);

                // Clean up partial file
                if (File.Exists(fullPath))
                    File.Delete(fullPath);

                throw;
            }
        }

        public async Task<Stream> GetFileAsync(string filePath)
        {
            var fullPath = Path.Combine(_settings.BasePath, filePath);

            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"File not found: {filePath}");

            try
            {
                var memory = new MemoryStream();
                using (var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    await stream.CopyToAsync(memory);
                }
                memory.Position = 0;

                return memory;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading file: {FilePath}", filePath);
                throw;
            }
        }

        public async Task<Stream> GetFileVersionAsync(Guid documentId, int version)
        {
            var documentPath = GetDocumentDirectory(documentId);
            var fullPath = Path.Combine(_settings.BasePath, documentPath);

            if (!Directory.Exists(fullPath))
                throw new DirectoryNotFoundException($"Document directory not found: {documentId}");

            // Find file with version prefix
            var files = Directory.GetFiles(fullPath, $"v{version}_*");

            if (!files.Any())
                throw new FileNotFoundException($"Version {version} not found for document {documentId}");

            var filePath = files.First().Replace(_settings.BasePath + Path.DirectorySeparatorChar, "");
            return await GetFileAsync(filePath);
        }

        public async Task DeleteFileAsync(string filePath)
        {
            var fullPath = Path.Combine(_settings.BasePath, filePath);

            if (!File.Exists(fullPath))
            {
                _logger.LogWarning("File not found for deletion: {FilePath}", filePath);
                return;
            }

            try
            {
                File.Delete(fullPath);
                _logger.LogInformation("File deleted: {FilePath}", filePath);

                // Delete thumbnail if exists
                var thumbnailPath = GetThumbnailPath(filePath);
                var fullThumbnailPath = Path.Combine(_settings.BasePath, thumbnailPath);
                if (File.Exists(fullThumbnailPath))
                {
                    File.Delete(fullThumbnailPath);
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
                throw;
            }
        }

        public async Task DeleteDocumentFilesAsync(Guid documentId)
        {
            var documentPath = GetDocumentDirectory(documentId);
            var fullPath = Path.Combine(_settings.BasePath, documentPath);

            if (!Directory.Exists(fullPath))
            {
                _logger.LogWarning("Document directory not found: {DocumentId}", documentId);
                return;
            }

            try
            {
                Directory.Delete(fullPath, recursive: true);
                _logger.LogInformation("Document files deleted: {DocumentId}", documentId);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document files: {DocumentId}", documentId);
                throw;
            }
        }

        public async Task<bool> FileExistsAsync(string filePath)
        {
            var fullPath = Path.Combine(_settings.BasePath, filePath);
            return await Task.FromResult(File.Exists(fullPath));
        }

        public async Task<long> GetFileSizeAsync(string filePath)
        {
            var fullPath = Path.Combine(_settings.BasePath, filePath);

            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"File not found: {filePath}");

            var fileInfo = new FileInfo(fullPath);
            return await Task.FromResult(fileInfo.Length);
        }

        public async Task<string> GenerateThumbnailAsync(string filePath, Guid documentId)
        {
            var fullPath = Path.Combine(_settings.BasePath, filePath);

            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"Source file not found: {filePath}");

            try
            {
                var thumbnailPath = GetThumbnailPath(filePath);
                var fullThumbnailPath = Path.Combine(_settings.BasePath, thumbnailPath);

                // Ensure thumbnail directory exists
                var thumbnailDir = Path.GetDirectoryName(fullThumbnailPath);
                EnsureDirectoryExists(thumbnailDir);

                if (IsImageFile(filePath))
                {
                    await GenerateImageThumbnailAsync(fullPath, fullThumbnailPath);
                }
                else if (IsPdfFile(filePath))
                {
                    await GeneratePdfThumbnailAsync(fullPath, fullThumbnailPath);
                }

                _logger.LogInformation("Thumbnail generated: {ThumbnailPath}", thumbnailPath);

                return thumbnailPath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating thumbnail: {FilePath}", filePath);
                // Don't throw - thumbnail generation is optional
                return null;
            }
        }

        public async Task<Dictionary<string, string>> ExtractMetadataAsync(IFormFile file)
        {
            var metadata = new Dictionary<string, string>
            {
                { "OriginalFileName", file.FileName },
                { "ContentType", file.ContentType },
                { "FileSize", file.Length.ToString() },
                { "Extension", Path.GetExtension(file.FileName).ToLower() }
            };

            try
            {
                // Extract additional metadata based on file type
                if (IsImageFile(file.FileName))
                {
                    var imageMetadata = await ExtractImageMetadataAsync(file);
                    foreach (var kvp in imageMetadata)
                    {
                        metadata[kvp.Key] = kvp.Value;
                    }
                }
                else if (IsPdfFile(file.FileName))
                {
                    var pdfMetadata = await ExtractPdfMetadataAsync(file);
                    foreach (var kvp in pdfMetadata)
                    {
                        metadata[kvp.Key] = kvp.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error extracting metadata from file: {FileName}", file.FileName);
            }

            return metadata;
        }

        public string GetStorageBasePath()
        {
            return _settings.BasePath;
        }

        public async Task<long> GetTotalStorageUsedAsync()
        {
            try
            {
                var documentsPath = Path.Combine(_settings.BasePath, "Documents");

                if (!Directory.Exists(documentsPath))
                    return 0;

                var directoryInfo = new DirectoryInfo(documentsPath);
                var totalSize = directoryInfo.EnumerateFiles("*", SearchOption.AllDirectories)
                    .Sum(file => file.Length);

                return await Task.FromResult(totalSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating storage usage");
                return 0;
            }
        }

        private string GenerateFilePath(Guid documentId, int version, string originalFileName)
        {
            var now = DateTime.UtcNow;
            var extension = Path.GetExtension(originalFileName);
            var sanitizedFileName = SanitizeFileName(Path.GetFileNameWithoutExtension(originalFileName));

            // Structure: Documents/{year}/{month}/{documentId}/v{version}_{filename}.ext
            return Path.Combine(
                "Documents",
                now.Year.ToString(),
                now.Month.ToString("D2"),
                documentId.ToString(),
                $"v{version}_{sanitizedFileName}{extension}"
            );
        }

        private string GetDocumentDirectory(Guid documentId)
        {
            var documentsPath = Path.Combine(_settings.BasePath, "Documents");

            // Search for document directory
            var directories = Directory.GetDirectories(documentsPath, documentId.ToString(), SearchOption.AllDirectories);

            if (!directories.Any())
                throw new DirectoryNotFoundException($"Document directory not found: {documentId}");

            return directories.First().Replace(_settings.BasePath + Path.DirectorySeparatorChar, "");
        }

        private string GetThumbnailPath(string filePath)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            return Path.Combine("Thumbnails", $"{fileName}_thumb.jpg");
        }

        private string SanitizeFileName(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));

            // Limit length
            if (sanitized.Length > 100)
                sanitized = sanitized.Substring(0, 100);

            return sanitized;
        }

        private void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                _logger.LogDebug("Directory created: {Path}", path);
            }
        }

        private bool IsImageFile(string fileName)
        {
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".svg" };
            var extension = Path.GetExtension(fileName).ToLower();
            return imageExtensions.Contains(extension);
        }

        private bool IsPdfFile(string fileName)
        {
            return Path.GetExtension(fileName).ToLower() == ".pdf";
        }

        private async Task GenerateImageThumbnailAsync(string sourcePath, string thumbnailPath)
        {
            // Implementation would use System.Drawing or ImageSharp
            // For now, placeholder
            await Task.CompletedTask;

            // Example with ImageSharp:
            // using (var image = await Image.LoadAsync(sourcePath))
            // {
            //     image.Mutate(x => x.Resize(new ResizeOptions
            //     {
            //         Size = new Size(200, 200),
            //         Mode = ResizeMode.Max
            //     }));
            //     await image.SaveAsJpegAsync(thumbnailPath);
            // }
        }

        private async Task GeneratePdfThumbnailAsync(string sourcePath, string thumbnailPath)
        {
            // Implementation would use a PDF library like PDFium or Ghostscript
            await Task.CompletedTask;
        }

        private async Task<Dictionary<string, string>> ExtractImageMetadataAsync(IFormFile file)
        {
            // Implementation would extract EXIF data, dimensions, etc.
            return await Task.FromResult(new Dictionary<string, string>());
        }

        private async Task<Dictionary<string, string>> ExtractPdfMetadataAsync(IFormFile file)
        {
            // Implementation would extract PDF metadata (author, pages, etc.)
            return await Task.FromResult(new Dictionary<string, string>());
        }
    }
}
```

## File Upload Middleware

### FileUploadValidationMiddleware

```csharp
using Microsoft.AspNetCore.Http;
using Backend.Models.Configuration;
using Microsoft.Extensions.Options;

namespace Backend.Middleware
{
    public class FileUploadValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly FileStorageSettings _settings;
        private readonly ILogger<FileUploadValidationMiddleware> _logger;

        public FileUploadValidationMiddleware(
            RequestDelegate next,
            IOptions<FileStorageSettings> settings,
            ILogger<FileUploadValidationMiddleware> logger)
        {
            _next = next;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Only validate POST requests with files
            if (context.Request.Method == "POST" && context.Request.HasFormContentType)
            {
                try
                {
                    var form = await context.Request.ReadFormAsync();

                    if (form.Files.Any())
                    {
                        foreach (var file in form.Files)
                        {
                            // Validate file size
                            if (file.Length > _settings.MaxFileSizeBytes)
                            {
                                context.Response.StatusCode = 413; // Payload Too Large
                                await context.Response.WriteAsJsonAsync(new
                                {
                                    error = $"File '{file.FileName}' exceeds maximum size of {_settings.MaxFileSizeBytes / 1048576} MB"
                                });
                                return;
                            }

                            // Validate MIME type
                            if (!IsAllowedMimeType(file.ContentType))
                            {
                                context.Response.StatusCode = 415; // Unsupported Media Type
                                await context.Response.WriteAsJsonAsync(new
                                {
                                    error = $"File type '{file.ContentType}' is not allowed"
                                });
                                return;
                            }

                            // Validate file extension
                            var extension = Path.GetExtension(file.FileName).ToLower();
                            if (!IsAllowedExtension(extension))
                            {
                                context.Response.StatusCode = 415;
                                await context.Response.WriteAsJsonAsync(new
                                {
                                    error = $"File extension '{extension}' is not allowed"
                                });
                                return;
                            }

                            _logger.LogDebug("File validated: {FileName}, Size: {Size}, Type: {ContentType}",
                                file.FileName, file.Length, file.ContentType);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error validating file upload");
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsJsonAsync(new { error = "Error validating file upload" });
                    return;
                }
            }

            await _next(context);
        }

        private bool IsAllowedMimeType(string mimeType)
        {
            var allowedTypes = new[]
            {
                // Documents
                "application/pdf",
                "application/msword",
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "application/vnd.ms-excel",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "application/vnd.ms-powerpoint",
                "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                "text/plain",
                "text/csv",

                // Images
                "image/jpeg",
                "image/png",
                "image/gif",
                "image/bmp",
                "image/webp",
                "image/svg+xml",

                // Archives
                "application/zip",
                "application/x-rar-compressed",
                "application/x-7z-compressed",

                // Other
                "application/json",
                "application/xml",
                "text/xml"
            };

            return allowedTypes.Contains(mimeType.ToLower());
        }

        private bool IsAllowedExtension(string extension)
        {
            var allowedExtensions = new[]
            {
                ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
                ".txt", ".csv", ".json", ".xml",
                ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".svg",
                ".zip", ".rar", ".7z"
            };

            return allowedExtensions.Contains(extension.ToLower());
        }
    }
}
```

## File Download Handler

### Enhanced DocumentsController (Download Methods)

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace Backend.Controllers
{
    public partial class DocumentsController
    {
        /// <summary>
        /// Download document file (current version)
        /// </summary>
        [HttpGet("{id}/download")]
        public async Task<IActionResult> DownloadDocument(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var (fileStream, fileName, mimeType) = await _documentService.DownloadDocumentAsync(id, userId);

                // Set content disposition for download
                var contentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = fileName
                };
                Response.Headers.Add(HeaderNames.ContentDisposition, contentDisposition.ToString());

                return File(fileStream, mimeType, fileName);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading document {DocumentId}", id);
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Download specific version of document
        /// </summary>
        [HttpGet("{id}/download/version/{versionNumber}")]
        public async Task<IActionResult> DownloadDocumentVersion(Guid id, int versionNumber)
        {
            try
            {
                var userId = GetCurrentUserId();
                var (fileStream, fileName, mimeType) = await _documentService.DownloadDocumentVersionAsync(
                    id, versionNumber, userId);

                var contentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = fileName
                };
                Response.Headers.Add(HeaderNames.ContentDisposition, contentDisposition.ToString());

                return File(fileStream, mimeType, fileName);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading document version {DocumentId} v{Version}", id, versionNumber);
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Stream document for preview (inline display)
        /// </summary>
        [HttpGet("{id}/preview")]
        public async Task<IActionResult> PreviewDocument(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var (fileStream, fileName, mimeType) = await _documentService.DownloadDocumentAsync(id, userId);

                // Set content disposition for inline display
                var contentDisposition = new ContentDispositionHeaderValue("inline")
                {
                    FileName = fileName
                };
                Response.Headers.Add(HeaderNames.ContentDisposition, contentDisposition.ToString());

                return File(fileStream, mimeType);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error previewing document {DocumentId}", id);
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Get document thumbnail
        /// </summary>
        [HttpGet("{id}/thumbnail")]
        public async Task<IActionResult> GetThumbnail(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var thumbnailStream = await _documentService.GetDocumentThumbnailAsync(id, userId);

                if (thumbnailStream == null)
                    return NotFound();

                return File(thumbnailStream, "image/jpeg");
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting thumbnail for document {DocumentId}", id);
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Bulk download multiple documents as ZIP
        /// </summary>
        [HttpPost("bulk-download")]
        public async Task<IActionResult> BulkDownload([FromBody] List<Guid> documentIds)
        {
            try
            {
                var userId = GetCurrentUserId();
                var zipStream = await _documentService.BulkDownloadDocumentsAsync(documentIds, userId);

                var fileName = $"documents_{DateTime.UtcNow:yyyyMMddHHmmss}.zip";

                return File(zipStream, "application/zip", fileName);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk downloading documents");
                return StatusCode(500);
            }
        }
    }
}
```

## Configuration Models

### FileStorageSettings

```csharp
namespace Backend.Models.Configuration
{
    public class FileStorageSettings
    {
        public string BasePath { get; set; }
        public long MaxFileSizeBytes { get; set; }
        public int ThumbnailWidth { get; set; }
        public int ThumbnailHeight { get; set; }
        public bool EnableVirusScanning { get; set; }
        public string[] AllowedExtensions { get; set; }
        public string[] BlockedExtensions { get; set; }
        public int FileCleanupDays { get; set; }
    }
}
```

### appsettings.json Configuration

```json
{
  "FileStorage": {
    "BasePath": "C:\\EDMStorage",
    "MaxFileSizeBytes": 104857600,
    "ThumbnailWidth": 200,
    "ThumbnailHeight": 200,
    "EnableVirusScanning": false,
    "AllowedExtensions": [
      ".pdf",
      ".doc",
      ".docx",
      ".xls",
      ".xlsx",
      ".ppt",
      ".pptx",
      ".txt",
      ".csv",
      ".json",
      ".xml",
      ".jpg",
      ".jpeg",
      ".png",
      ".gif",
      ".bmp",
      ".webp",
      ".zip",
      ".rar"
    ],
    "BlockedExtensions": [
      ".exe",
      ".dll",
      ".bat",
      ".cmd",
      ".sh",
      ".ps1",
      ".msi",
      ".jar",
      ".war",
      ".ear"
    ],
    "FileCleanupDays": 90
  }
}
```

## Additional Service Methods for DocumentService

```csharp
public async Task<(Stream fileStream, string fileName, string mimeType)> DownloadDocumentVersionAsync(
    Guid documentId,
    int versionNumber,
    Guid userId)
{
    var document = await _context.Documents
        .FirstOrDefaultAsync(d => d.Id == documentId && !d.IsDeleted);

    if (document == null)
        throw new KeyNotFoundException("Document not found");

    // Check permissions
    if (!document.IsPublic && document.OwnerId != userId)
    {
        var hasAccess = await _permissionService.CanAccessDocumentAsync(documentId, userId, "Read");
        if (!hasAccess)
            throw new UnauthorizedAccessException("You don't have permission to download this document");
    }

    var fileStream = await _fileStorageService.GetFileVersionAsync(documentId, versionNumber);

    // Increment download count
    document.DownloadCount++;
    await _context.SaveChangesAsync();

    // Audit log
    await _auditService.LogActionAsync("Download", "Document", document.Id, userId,
        $"Downloaded document version {versionNumber}: {document.Title}");

    return (fileStream, document.FileName, document.MimeType);
}

public async Task<Stream> GetDocumentThumbnailAsync(Guid documentId, Guid userId)
{
    var document = await _context.Documents
        .FirstOrDefaultAsync(d => d.Id == documentId && !d.IsDeleted);

    if (document == null)
        throw new KeyNotFoundException("Document not found");

    // Check permissions
    var hasAccess = await _permissionService.CanAccessDocumentAsync(documentId, userId, "Read");
    if (!hasAccess && document.OwnerId != userId && !document.IsPublic)
        throw new UnauthorizedAccessException("You don't have permission to view this document");

    var version = await _context.DocumentVersions
        .FirstOrDefaultAsync(v => v.DocumentId == documentId && v.IsCurrentVersion);

    if (version == null)
        return null;

    var thumbnailPath = await _fileStorageService.GenerateThumbnailAsync(version.FilePath, documentId);

    if (string.IsNullOrEmpty(thumbnailPath))
        return null;

    return await _fileStorageService.GetFileAsync(thumbnailPath);
}

public async Task<Stream> BulkDownloadDocumentsAsync(List<Guid> documentIds, Guid userId)
{
    var memoryStream = new MemoryStream();

    using (var archive = new System.IO.Compression.ZipArchive(memoryStream, System.IO.Compression.ZipArchiveMode.Create, true))
    {
        foreach (var documentId in documentIds)
        {
            try
            {
                var (fileStream, fileName, _) = await DownloadDocumentAsync(documentId, userId);

                var entry = archive.CreateEntry(fileName);
                using (var entryStream = entry.Open())
                {
                    await fileStream.CopyToAsync(entryStream);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to add document {DocumentId} to ZIP", documentId);
                // Continue with other documents
            }
        }
    }

    memoryStream.Position = 0;
    return memoryStream;
}
```

## Virus Scanning Integration (Optional)

### IVirusScannerService

```csharp
namespace Backend.Services.Interfaces
{
    public interface IVirusScannerService
    {
        Task<bool> ScanFileAsync(Stream fileStream, string fileName);
        Task<ScanResult> ScanFileDetailedAsync(Stream fileStream, string fileName);
    }

    public class ScanResult
    {
        public bool IsClean { get; set; }
        public string ThreatName { get; set; }
        public string ScannerVersion { get; set; }
        public DateTime ScannedAt { get; set; }
    }
}
```

### ClamAVScannerService (Example)

```csharp
namespace Backend.Services
{
    public class ClamAVScannerService : IVirusScannerService
    {
        private readonly ILogger<ClamAVScannerService> _logger;
        private readonly string _clamAVHost;
        private readonly int _clamAVPort;

        public ClamAVScannerService(
            IConfiguration configuration,
            ILogger<ClamAVScannerService> logger)
        {
            _logger = logger;
            _clamAVHost = configuration["ClamAV:Host"] ?? "localhost";
            _clamAVPort = int.Parse(configuration["ClamAV:Port"] ?? "3310");
        }

        public async Task<bool> ScanFileAsync(Stream fileStream, string fileName)
        {
            try
            {
                // Implementation would connect to ClamAV daemon
                // For now, placeholder
                await Task.CompletedTask;
                return true; // Clean
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scanning file {FileName}", fileName);
                throw;
            }
        }

        public async Task<ScanResult> ScanFileDetailedAsync(Stream fileStream, string fileName)
        {
            var isClean = await ScanFileAsync(fileStream, fileName);

            return new ScanResult
            {
                IsClean = isClean,
                ThreatName = isClean ? null : "Unknown",
                ScannerVersion = "ClamAV 1.0",
                ScannedAt = DateTime.UtcNow
            };
        }
    }
}
```

## Program.cs Registration

```csharp
// File Storage Configuration
builder.Services.Configure<FileStorageSettings>(
    builder.Configuration.GetSection("FileStorage"));

// Services
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IVirusScannerService, ClamAVScannerService>();

// Middleware
app.UseMiddleware<FileUploadValidationMiddleware>();

// Configure request size limits
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; // 100 MB
});

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 104857600; // 100 MB
});
```

## File Upload Example (Frontend)

### TypeScript/Angular

```typescript
uploadDocument(file: File, metadata: DocumentMetadata): Observable<DocumentDTO> {
  const formData = new FormData();
  formData.append('file', file);
  formData.append('title', metadata.title);
  formData.append('description', metadata.description);
  formData.append('folderId', metadata.folderId);

  return this.http.post<ApiResponse<DocumentDTO>>(
    `${this.apiUrl}/documents`,
    formData,
    {
      reportProgress: true,
      observe: 'events'
    }
  ).pipe(
    map(event => {
      if (event.type === HttpEventType.UploadProgress) {
        const progress = Math.round(100 * event.loaded / event.total);
        this.uploadProgress$.next(progress);
      } else if (event.type === HttpEventType.Response) {
        return event.body.data;
      }
    }),
    filter(data => data !== undefined)
  );
}
```

## Security Best Practices

1. **Validate file types** - Check both MIME type and extension
2. **Scan for viruses** - Integrate antivirus scanning
3. **Limit file sizes** - Prevent DoS attacks
4. **Sanitize filenames** - Remove special characters
5. **Use streaming** - Don't load entire file into memory
6. **Separate storage** - Store files outside web root
7. **Access control** - Always verify permissions before serving files
8. **Audit logging** - Track all file operations
9. **Encryption at rest** - Consider encrypting stored files
10. **Backup strategy** - Regular backups of file storage

---

**Document Version**: 1.0  
**Last Updated**: November 25, 2025  
**Author**: EDM Project Team  
**Status**: Design Complete - Ready for Implementation

## Next Steps

1. **Install required packages**: System.Drawing.Common or SixLabors.ImageSharp for thumbnails
2. **Set up ClamAV** (optional) for virus scanning
3. **Configure storage paths** in appsettings.json
4. **Implement thumbnail generation** for images and PDFs
5. **Add metadata extraction** using appropriate libraries
6. **Test with large files** to verify streaming and performance
7. **Implement cleanup jobs** for orphaned files
8. **Set up CDN** (optional) for serving static files at scale
