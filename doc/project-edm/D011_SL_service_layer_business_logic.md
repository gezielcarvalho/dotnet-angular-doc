# EDM System - Service Layer & Business Logic

## Service Architecture Overview

This document defines the service layer that implements business logic, validation rules, and orchestrates operations between controllers and the data access layer.

## Architecture Diagram

```
┌──────────────────────────────────────────────────────────────┐
│                    Service Layer Architecture                 │
│                                                               │
│  Controllers                                                  │
│       ↓                                                       │
│  ┌─────────────────────────────────────────────────────┐    │
│  │              Service Interfaces (DI)                 │    │
│  └─────────────────────────────────────────────────────┘    │
│       ↓                                                       │
│  ┌─────────────────────────────────────────────────────┐    │
│  │         Service Implementations                      │    │
│  │  • Business Logic                                    │    │
│  │  • Validation Rules                                  │    │
│  │  • Permission Checking                               │    │
│  │  • Transaction Management                            │    │
│  │  • File Operations                                   │    │
│  │  • Audit Logging                                     │    │
│  └─────────────────────────────────────────────────────┘    │
│       ↓                                                       │
│  ┌─────────────────────────────────────────────────────┐    │
│  │         Data Access Layer (EF Core)                  │    │
│  │  • DbContext                                         │    │
│  │  • Repositories                                      │    │
│  │  • Database Operations                               │    │
│  └─────────────────────────────────────────────────────┘    │
│       ↓                                                       │
│  Database + File System                                      │
└──────────────────────────────────────────────────────────────┘
```

## Core Service Interfaces

### IDocumentService

```csharp
using Backend.Models.DTO;
using Backend.Models.DTO.Common;

namespace Backend.Services.Interfaces
{
    public interface IDocumentService
    {
        Task<PagedResponse<DocumentDTO>> GetDocumentsAsync(DocumentSearchFilter filter, Guid userId);
        Task<DocumentDTO> GetDocumentByIdAsync(Guid id, Guid userId);
        Task<DocumentDTO> CreateDocumentAsync(CreateDocumentDTO dto, Guid userId);
        Task<DocumentDTO> UpdateDocumentAsync(Guid id, UpdateDocumentDTO dto, Guid userId);
        Task DeleteDocumentAsync(Guid id, Guid userId);
        Task<(Stream fileStream, string fileName, string mimeType)> DownloadDocumentAsync(Guid id, Guid userId);
        Task<List<DocumentVersionDTO>> GetDocumentVersionsAsync(Guid id, Guid userId);
        Task<DocumentVersionDTO> CreateDocumentVersionAsync(Guid id, IFormFile file, string comment, Guid userId);
        Task<List<DocumentDTO>> SearchDocumentsAsync(string searchTerm, Guid userId);
    }
}
```

### IFolderService

```csharp
using Backend.Models.DTO;

namespace Backend.Services.Interfaces
{
    public interface IFolderService
    {
        Task<List<FolderTreeDTO>> GetFolderTreeAsync(Guid userId);
        Task<FolderDTO> GetFolderByIdAsync(Guid id, Guid userId);
        Task<FolderDTO> CreateFolderAsync(CreateFolderDTO dto, Guid userId);
        Task<FolderDTO> UpdateFolderAsync(Guid id, UpdateFolderDTO dto, Guid userId);
        Task DeleteFolderAsync(Guid id, Guid userId);
        Task<string> GetFolderPathAsync(Guid folderId);
    }
}
```

### IAuthService

```csharp
using Backend.Models.DTO;

namespace Backend.Services.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDTO> LoginAsync(LoginDTO dto);
        Task<UserDTO> GetUserByIdAsync(Guid id);
        Task<UserDTO> CreateUserAsync(CreateUserDTO dto);
        Task UpdateLastLoginAsync(Guid userId);
        Task<bool> ValidatePasswordAsync(string username, string password);
        string GenerateJwtToken(User user);
    }
}
```

### IPermissionService

```csharp
using Backend.Models.DTO;

namespace Backend.Services.Interfaces
{
    public interface IPermissionService
    {
        Task<bool> CanAccessDocumentAsync(Guid documentId, Guid userId, string permissionType);
        Task<bool> CanAccessFolderAsync(Guid folderId, Guid userId, string permissionType);
        Task<List<PermissionDTO>> GetDocumentPermissionsAsync(Guid documentId, Guid userId);
        Task<List<PermissionDTO>> GetFolderPermissionsAsync(Guid folderId, Guid userId);
        Task<PermissionDTO> GrantPermissionAsync(GrantPermissionDTO dto, Guid grantedBy);
        Task RevokePermissionAsync(Guid permissionId, Guid userId);
        Task InheritFolderPermissionsAsync(Guid folderId, Guid documentId);
    }
}
```

### IWorkflowService

```csharp
using Backend.Models.DTO;
using Backend.Models.DTO.Common;

namespace Backend.Services.Interfaces
{
    public interface IWorkflowService
    {
        Task<PagedResponse<WorkflowDTO>> GetWorkflowsAsync(PaginationFilter filter, Guid userId);
        Task<WorkflowDTO> GetWorkflowByIdAsync(Guid id, Guid userId);
        Task<WorkflowDTO> CreateWorkflowAsync(CreateWorkflowDTO dto, Guid userId);
        Task<WorkflowStepDTO> CompleteWorkflowStepAsync(Guid workflowId, Guid stepId, CompleteWorkflowStepDTO dto, Guid userId);
        Task CancelWorkflowAsync(Guid workflowId, Guid userId);
    }
}
```

### IFileStorageService

```csharp
namespace Backend.Services.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(IFormFile file, Guid documentId, int version);
        Task<Stream> GetFileAsync(string filePath);
        Task DeleteFileAsync(string filePath);
        Task<bool> FileExistsAsync(string filePath);
        Task<long> GetFileSizeAsync(string filePath);
    }
}
```

### IAuditService

```csharp
using Backend.Models;

namespace Backend.Services.Interfaces
{
    public interface IAuditService
    {
        Task LogActionAsync(string action, string entityType, Guid entityId, Guid userId, string details = null);
        Task<List<AuditLog>> GetEntityAuditLogsAsync(string entityType, Guid entityId);
    }
}
```

## Service Implementations

### DocumentService

```csharp
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Backend.Data;
using Backend.Models;
using Backend.Models.DTO;
using Backend.Models.DTO.Common;
using Backend.Services.Interfaces;

namespace Backend.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly EdmDbContext _context;
        private readonly IMapper _mapper;
        private readonly IPermissionService _permissionService;
        private readonly IFileStorageService _fileStorageService;
        private readonly IAuditService _auditService;
        private readonly ILogger<DocumentService> _logger;

        public DocumentService(
            EdmDbContext context,
            IMapper mapper,
            IPermissionService permissionService,
            IFileStorageService fileStorageService,
            IAuditService auditService,
            ILogger<DocumentService> logger)
        {
            _context = context;
            _mapper = mapper;
            _permissionService = permissionService;
            _fileStorageService = fileStorageService;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<PagedResponse<DocumentDTO>> GetDocumentsAsync(
            DocumentSearchFilter filter,
            Guid userId)
        {
            var query = _context.Documents
                .Include(d => d.Folder)
                .Include(d => d.Owner)
                .Include(d => d.DocumentTags)
                    .ThenInclude(dt => dt.Tag)
                .Where(d => !d.IsDeleted);

            // Apply filters
            if (filter.FolderId.HasValue)
                query = query.Where(d => d.FolderId == filter.FolderId.Value);

            if (!string.IsNullOrEmpty(filter.Status))
                query = query.Where(d => d.Status == filter.Status);

            if (filter.Tags != null && filter.Tags.Any())
                query = query.Where(d => d.DocumentTags.Any(dt => filter.Tags.Contains(dt.Tag.Name)));

            if (filter.CreatedFrom.HasValue)
                query = query.Where(d => d.CreatedAt >= filter.CreatedFrom.Value);

            if (filter.CreatedTo.HasValue)
                query = query.Where(d => d.CreatedAt <= filter.CreatedTo.Value);

            if (!string.IsNullOrEmpty(filter.FileExtension))
                query = query.Where(d => d.FileExtension == filter.FileExtension);

            if (filter.OwnerId.HasValue)
                query = query.Where(d => d.OwnerId == filter.OwnerId.Value);

            // Search term
            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                query = query.Where(d =>
                    d.Title.Contains(filter.SearchTerm) ||
                    d.Description.Contains(filter.SearchTerm) ||
                    d.FileName.Contains(filter.SearchTerm));
            }

            // Filter by permissions - only show documents user can read
            var accessibleDocuments = query.ToList()
                .Where(d => d.IsPublic || d.OwnerId == userId ||
                    _permissionService.CanAccessDocumentAsync(d.Id, userId, "Read").Result)
                .AsQueryable();

            // Total count
            var totalCount = accessibleDocuments.Count();

            // Sorting
            if (!string.IsNullOrEmpty(filter.SortBy))
            {
                query = filter.SortBy.ToLower() switch
                {
                    "title" => filter.SortDescending
                        ? accessibleDocuments.OrderByDescending(d => d.Title)
                        : accessibleDocuments.OrderBy(d => d.Title),
                    "createdat" => filter.SortDescending
                        ? accessibleDocuments.OrderByDescending(d => d.CreatedAt)
                        : accessibleDocuments.OrderBy(d => d.CreatedAt),
                    "filesize" => filter.SortDescending
                        ? accessibleDocuments.OrderByDescending(d => d.FileSize)
                        : accessibleDocuments.OrderBy(d => d.FileSize),
                    _ => accessibleDocuments.OrderByDescending(d => d.CreatedAt)
                };
            }
            else
            {
                accessibleDocuments = accessibleDocuments.OrderByDescending(d => d.CreatedAt);
            }

            // Pagination
            var documents = accessibleDocuments
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            var documentDtos = documents.Select(d => MapToDocumentDTO(d)).ToList();

            return new PagedResponse<DocumentDTO>
            {
                Items = documentDtos,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<DocumentDTO> GetDocumentByIdAsync(Guid id, Guid userId)
        {
            var document = await _context.Documents
                .Include(d => d.Folder)
                .Include(d => d.Owner)
                .Include(d => d.DocumentTags)
                    .ThenInclude(dt => dt.Tag)
                .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

            if (document == null)
                return null;

            // Check permissions
            if (!document.IsPublic && document.OwnerId != userId)
            {
                var hasAccess = await _permissionService.CanAccessDocumentAsync(id, userId, "Read");
                if (!hasAccess)
                    throw new UnauthorizedAccessException("You don't have permission to access this document");
            }

            // Increment view count
            document.ViewCount++;
            await _context.SaveChangesAsync();

            return MapToDocumentDTO(document);
        }

        public async Task<DocumentDTO> CreateDocumentAsync(CreateDocumentDTO dto, Guid userId)
        {
            // Validate folder access
            var hasAccess = await _permissionService.CanAccessFolderAsync(dto.FolderId, userId, "Write");
            if (!hasAccess)
                throw new UnauthorizedAccessException("You don't have permission to upload to this folder");

            // Validate file
            if (dto.File == null || dto.File.Length == 0)
                throw new ArgumentException("File is required");

            if (dto.File.Length > 104857600) // 100 MB limit
                throw new ArgumentException("File size exceeds maximum limit of 100 MB");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Create document entity
                var document = new Document
                {
                    Id = Guid.NewGuid(),
                    Title = dto.Title,
                    Description = dto.Description,
                    FolderId = dto.FolderId,
                    FileName = dto.File.FileName,
                    FileSize = dto.File.Length,
                    MimeType = dto.File.ContentType,
                    FileExtension = Path.GetExtension(dto.File.FileName).ToLower(),
                    CurrentVersion = 1,
                    Status = "Active",
                    OwnerId = userId,
                    IsPublic = dto.IsPublic,
                    CustomMetadata = dto.CustomMetadata,
                    ExpirationDate = dto.ExpirationDate,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId.ToString()
                };

                _context.Documents.Add(document);
                await _context.SaveChangesAsync();

                // Save file to storage
                var filePath = await _fileStorageService.SaveFileAsync(dto.File, document.Id, 1);

                // Create first version
                var version = new DocumentVersion
                {
                    Id = Guid.NewGuid(),
                    DocumentId = document.Id,
                    VersionNumber = 1,
                    FilePath = filePath,
                    FileSize = dto.File.Length,
                    MimeType = dto.File.ContentType,
                    VersionComment = "Initial version",
                    IsCurrentVersion = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId.ToString()
                };

                _context.DocumentVersions.Add(version);

                // Add tags
                if (dto.Tags != null && dto.Tags.Any())
                {
                    foreach (var tagName in dto.Tags)
                    {
                        var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == tagName);
                        if (tag == null)
                        {
                            tag = new Tag
                            {
                                Id = Guid.NewGuid(),
                                Name = tagName,
                                CreatedAt = DateTime.UtcNow,
                                CreatedBy = userId.ToString()
                            };
                            _context.Tags.Add(tag);
                        }

                        _context.DocumentTags.Add(new DocumentTag
                        {
                            DocumentId = document.Id,
                            TagId = tag.Id
                        });
                    }
                }

                await _context.SaveChangesAsync();

                // Inherit folder permissions
                await _permissionService.InheritFolderPermissionsAsync(dto.FolderId, document.Id);

                // Audit log
                await _auditService.LogActionAsync("Create", "Document", document.Id, userId,
                    $"Created document: {document.Title}");

                await transaction.CommitAsync();

                _logger.LogInformation("Document {DocumentId} created by user {UserId}", document.Id, userId);

                return await GetDocumentByIdAsync(document.Id, userId);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating document");
                throw;
            }
        }

        public async Task<DocumentDTO> UpdateDocumentAsync(Guid id, UpdateDocumentDTO dto, Guid userId)
        {
            var document = await _context.Documents
                .Include(d => d.DocumentTags)
                .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

            if (document == null)
                throw new KeyNotFoundException("Document not found");

            // Check permissions
            var hasAccess = await _permissionService.CanAccessDocumentAsync(id, userId, "Write");
            if (!hasAccess && document.OwnerId != userId)
                throw new UnauthorizedAccessException("You don't have permission to update this document");

            // Update properties
            if (!string.IsNullOrEmpty(dto.Title))
                document.Title = dto.Title;

            if (dto.Description != null)
                document.Description = dto.Description;

            if (dto.FolderId.HasValue && dto.FolderId.Value != document.FolderId)
            {
                // Validate access to new folder
                var folderAccess = await _permissionService.CanAccessFolderAsync(dto.FolderId.Value, userId, "Write");
                if (!folderAccess)
                    throw new UnauthorizedAccessException("You don't have permission to move to this folder");

                document.FolderId = dto.FolderId.Value;
            }

            if (dto.IsPublic.HasValue)
                document.IsPublic = dto.IsPublic.Value;

            if (dto.CustomMetadata != null)
                document.CustomMetadata = dto.CustomMetadata;

            if (dto.ExpirationDate.HasValue)
                document.ExpirationDate = dto.ExpirationDate;

            // Update tags
            if (dto.Tags != null)
            {
                // Remove existing tags
                _context.DocumentTags.RemoveRange(document.DocumentTags);

                // Add new tags
                foreach (var tagName in dto.Tags)
                {
                    var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == tagName);
                    if (tag == null)
                    {
                        tag = new Tag
                        {
                            Id = Guid.NewGuid(),
                            Name = tagName,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = userId.ToString()
                        };
                        _context.Tags.Add(tag);
                    }

                    _context.DocumentTags.Add(new DocumentTag
                    {
                        DocumentId = document.Id,
                        TagId = tag.Id
                    });
                }
            }

            document.ModifiedAt = DateTime.UtcNow;
            document.ModifiedBy = userId.ToString();

            await _context.SaveChangesAsync();

            // Audit log
            await _auditService.LogActionAsync("Update", "Document", document.Id, userId,
                $"Updated document: {document.Title}");

            return await GetDocumentByIdAsync(id, userId);
        }

        public async Task DeleteDocumentAsync(Guid id, Guid userId)
        {
            var document = await _context.Documents
                .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

            if (document == null)
                throw new KeyNotFoundException("Document not found");

            // Check permissions - only owner or admin can delete
            var hasAccess = await _permissionService.CanAccessDocumentAsync(id, userId, "Delete");
            if (!hasAccess && document.OwnerId != userId)
                throw new UnauthorizedAccessException("You don't have permission to delete this document");

            // Soft delete
            document.IsDeleted = true;
            document.DeletedAt = DateTime.UtcNow;
            document.DeletedBy = userId.ToString();

            await _context.SaveChangesAsync();

            // Audit log
            await _auditService.LogActionAsync("Delete", "Document", document.Id, userId,
                $"Deleted document: {document.Title}");

            _logger.LogInformation("Document {DocumentId} deleted by user {UserId}", id, userId);
        }

        public async Task<(Stream fileStream, string fileName, string mimeType)> DownloadDocumentAsync(
            Guid id,
            Guid userId)
        {
            var document = await _context.Documents
                .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

            if (document == null)
                throw new KeyNotFoundException("Document not found");

            // Check permissions
            if (!document.IsPublic && document.OwnerId != userId)
            {
                var hasAccess = await _permissionService.CanAccessDocumentAsync(id, userId, "Read");
                if (!hasAccess)
                    throw new UnauthorizedAccessException("You don't have permission to download this document");
            }

            // Get current version file path
            var version = await _context.DocumentVersions
                .FirstOrDefaultAsync(v => v.DocumentId == id && v.IsCurrentVersion);

            if (version == null)
                throw new InvalidOperationException("No version found for document");

            var fileStream = await _fileStorageService.GetFileAsync(version.FilePath);

            // Increment download count
            document.DownloadCount++;
            await _context.SaveChangesAsync();

            // Audit log
            await _auditService.LogActionAsync("Download", "Document", document.Id, userId,
                $"Downloaded document: {document.Title}");

            return (fileStream, document.FileName, document.MimeType);
        }

        public async Task<List<DocumentVersionDTO>> GetDocumentVersionsAsync(Guid id, Guid userId)
        {
            var document = await _context.Documents
                .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

            if (document == null)
                throw new KeyNotFoundException("Document not found");

            // Check permissions
            var hasAccess = await _permissionService.CanAccessDocumentAsync(id, userId, "Read");
            if (!hasAccess && document.OwnerId != userId && !document.IsPublic)
                throw new UnauthorizedAccessException("You don't have permission to view document versions");

            var versions = await _context.DocumentVersions
                .Where(v => v.DocumentId == id)
                .OrderByDescending(v => v.VersionNumber)
                .ToListAsync();

            return _mapper.Map<List<DocumentVersionDTO>>(versions);
        }

        public async Task<DocumentVersionDTO> CreateDocumentVersionAsync(
            Guid id,
            IFormFile file,
            string comment,
            Guid userId)
        {
            var document = await _context.Documents
                .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

            if (document == null)
                throw new KeyNotFoundException("Document not found");

            // Check permissions
            var hasAccess = await _permissionService.CanAccessDocumentAsync(id, userId, "Write");
            if (!hasAccess && document.OwnerId != userId)
                throw new UnauthorizedAccessException("You don't have permission to create new versions");

            // Validate file
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is required");

            // File extension should match original
            var newExtension = Path.GetExtension(file.FileName).ToLower();
            if (newExtension != document.FileExtension)
                throw new ArgumentException("New version must have the same file type as the original");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Mark current version as not current
                var currentVersion = await _context.DocumentVersions
                    .FirstOrDefaultAsync(v => v.DocumentId == id && v.IsCurrentVersion);

                if (currentVersion != null)
                    currentVersion.IsCurrentVersion = false;

                // Create new version
                var newVersionNumber = document.CurrentVersion + 1;
                var filePath = await _fileStorageService.SaveFileAsync(file, id, newVersionNumber);

                var newVersion = new DocumentVersion
                {
                    Id = Guid.NewGuid(),
                    DocumentId = id,
                    VersionNumber = newVersionNumber,
                    FilePath = filePath,
                    FileSize = file.Length,
                    MimeType = file.ContentType,
                    VersionComment = comment ?? $"Version {newVersionNumber}",
                    IsCurrentVersion = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId.ToString()
                };

                _context.DocumentVersions.Add(newVersion);

                // Update document
                document.CurrentVersion = newVersionNumber;
                document.FileSize = file.Length;
                document.ModifiedAt = DateTime.UtcNow;
                document.ModifiedBy = userId.ToString();

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Audit log
                await _auditService.LogActionAsync("CreateVersion", "Document", document.Id, userId,
                    $"Created version {newVersionNumber} for document: {document.Title}");

                return _mapper.Map<DocumentVersionDTO>(newVersion);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating document version");
                throw;
            }
        }

        public async Task<List<DocumentDTO>> SearchDocumentsAsync(string searchTerm, Guid userId)
        {
            // Full-text search implementation
            var documents = await _context.Documents
                .Include(d => d.Folder)
                .Include(d => d.Owner)
                .Include(d => d.DocumentTags)
                    .ThenInclude(dt => dt.Tag)
                .Where(d => !d.IsDeleted &&
                    (d.Title.Contains(searchTerm) ||
                     d.Description.Contains(searchTerm) ||
                     d.FileName.Contains(searchTerm) ||
                     d.DocumentTags.Any(dt => dt.Tag.Name.Contains(searchTerm))))
                .ToListAsync();

            // Filter by permissions
            var accessibleDocs = documents
                .Where(d => d.IsPublic || d.OwnerId == userId ||
                    _permissionService.CanAccessDocumentAsync(d.Id, userId, "Read").Result)
                .ToList();

            return accessibleDocs.Select(d => MapToDocumentDTO(d)).ToList();
        }

        private DocumentDTO MapToDocumentDTO(Document document)
        {
            return new DocumentDTO
            {
                Id = document.Id,
                Title = document.Title,
                Description = document.Description,
                FolderId = document.FolderId,
                FolderPath = document.Folder?.Path,
                FileName = document.FileName,
                FileSize = document.FileSize,
                MimeType = document.MimeType,
                FileExtension = document.FileExtension,
                CurrentVersion = document.CurrentVersion,
                Status = document.Status,
                OwnerId = document.OwnerId,
                OwnerName = $"{document.Owner?.FirstName} {document.Owner?.LastName}",
                IsPublic = document.IsPublic,
                Tags = document.DocumentTags?.Select(dt => dt.Tag.Name).ToList() ?? new List<string>(),
                ViewCount = document.ViewCount,
                DownloadCount = document.DownloadCount,
                ExpirationDate = document.ExpirationDate,
                CreatedAt = document.CreatedAt,
                CreatedBy = document.CreatedBy,
                ModifiedAt = document.ModifiedAt,
                ModifiedBy = document.ModifiedBy
            };
        }
    }
}
```

### FolderService

```csharp
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Backend.Data;
using Backend.Models;
using Backend.Models.DTO;
using Backend.Services.Interfaces;

namespace Backend.Services
{
    public class FolderService : IFolderService
    {
        private readonly EdmDbContext _context;
        private readonly IMapper _mapper;
        private readonly IPermissionService _permissionService;
        private readonly IAuditService _auditService;
        private readonly ILogger<FolderService> _logger;

        public FolderService(
            EdmDbContext context,
            IMapper mapper,
            IPermissionService permissionService,
            IAuditService auditService,
            ILogger<FolderService> logger)
        {
            _context = context;
            _mapper = mapper;
            _permissionService = permissionService;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<List<FolderTreeDTO>> GetFolderTreeAsync(Guid userId)
        {
            var allFolders = await _context.Folders
                .Where(f => !f.IsDeleted)
                .Include(f => f.Documents.Where(d => !d.IsDeleted))
                .ToListAsync();

            // Filter by permissions
            var accessibleFolders = allFolders
                .Where(f => f.OwnerId == userId ||
                    _permissionService.CanAccessFolderAsync(f.Id, userId, "Read").Result)
                .ToList();

            // Build tree structure
            var rootFolders = accessibleFolders
                .Where(f => f.ParentFolderId == null)
                .Select(f => BuildFolderTree(f, accessibleFolders))
                .ToList();

            return rootFolders;
        }

        public async Task<FolderDTO> GetFolderByIdAsync(Guid id, Guid userId)
        {
            var folder = await _context.Folders
                .Include(f => f.Owner)
                .Include(f => f.Documents.Where(d => !d.IsDeleted))
                .Include(f => f.SubFolders.Where(sf => !sf.IsDeleted))
                .FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted);

            if (folder == null)
                return null;

            // Check permissions
            var hasAccess = await _permissionService.CanAccessFolderAsync(id, userId, "Read");
            if (!hasAccess && folder.OwnerId != userId)
                throw new UnauthorizedAccessException("You don't have permission to access this folder");

            return new FolderDTO
            {
                Id = folder.Id,
                Name = folder.Name,
                Description = folder.Description,
                ParentFolderId = folder.ParentFolderId,
                Path = folder.Path,
                Level = folder.Level,
                IsSystemFolder = folder.IsSystemFolder,
                OwnerId = folder.OwnerId,
                OwnerName = $"{folder.Owner?.FirstName} {folder.Owner?.LastName}",
                DocumentCount = folder.Documents.Count,
                SubFolderCount = folder.SubFolders.Count,
                CreatedAt = folder.CreatedAt,
                CreatedBy = folder.CreatedBy,
                ModifiedAt = folder.ModifiedAt,
                ModifiedBy = folder.ModifiedBy
            };
        }

        public async Task<FolderDTO> CreateFolderAsync(CreateFolderDTO dto, Guid userId)
        {
            // Validate parent folder if specified
            if (dto.ParentFolderId.HasValue)
            {
                var parentFolder = await _context.Folders
                    .FirstOrDefaultAsync(f => f.Id == dto.ParentFolderId.Value && !f.IsDeleted);

                if (parentFolder == null)
                    throw new KeyNotFoundException("Parent folder not found");

                // Check permission to create in parent folder
                var hasAccess = await _permissionService.CanAccessFolderAsync(
                    dto.ParentFolderId.Value, userId, "Write");

                if (!hasAccess && parentFolder.OwnerId != userId)
                    throw new UnauthorizedAccessException("You don't have permission to create folders here");
            }

            // Check for duplicate name in same parent
            var exists = await _context.Folders
                .AnyAsync(f => f.Name == dto.Name &&
                    f.ParentFolderId == dto.ParentFolderId &&
                    !f.IsDeleted);

            if (exists)
                throw new InvalidOperationException("A folder with this name already exists in this location");

            // Calculate path and level
            var path = "/";
            var level = 0;

            if (dto.ParentFolderId.HasValue)
            {
                var parentPath = await GetFolderPathAsync(dto.ParentFolderId.Value);
                path = $"{parentPath}{dto.Name}/";
                var parent = await _context.Folders.FindAsync(dto.ParentFolderId.Value);
                level = parent.Level + 1;
            }
            else
            {
                path = $"/{dto.Name}/";
            }

            var folder = new Folder
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                ParentFolderId = dto.ParentFolderId,
                Path = path,
                Level = level,
                IsSystemFolder = false,
                OwnerId = userId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId.ToString()
            };

            _context.Folders.Add(folder);
            await _context.SaveChangesAsync();

            // Inherit parent folder permissions
            if (dto.ParentFolderId.HasValue)
            {
                var parentPermissions = await _context.Permissions
                    .Where(p => p.FolderId == dto.ParentFolderId.Value)
                    .ToListAsync();

                foreach (var parentPerm in parentPermissions)
                {
                    var newPerm = new Permission
                    {
                        Id = Guid.NewGuid(),
                        FolderId = folder.Id,
                        UserId = parentPerm.UserId,
                        PermissionType = parentPerm.PermissionType,
                        IsInherited = true,
                        GrantedAt = DateTime.UtcNow,
                        GrantedBy = userId.ToString()
                    };
                    _context.Permissions.Add(newPerm);
                }

                await _context.SaveChangesAsync();
            }

            // Audit log
            await _auditService.LogActionAsync("Create", "Folder", folder.Id, userId,
                $"Created folder: {folder.Name}");

            return await GetFolderByIdAsync(folder.Id, userId);
        }

        public async Task<FolderDTO> UpdateFolderAsync(Guid id, UpdateFolderDTO dto, Guid userId)
        {
            var folder = await _context.Folders
                .FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted);

            if (folder == null)
                throw new KeyNotFoundException("Folder not found");

            if (folder.IsSystemFolder)
                throw new InvalidOperationException("Cannot modify system folders");

            // Check permissions
            var hasAccess = await _permissionService.CanAccessFolderAsync(id, userId, "Write");
            if (!hasAccess && folder.OwnerId != userId)
                throw new UnauthorizedAccessException("You don't have permission to update this folder");

            if (!string.IsNullOrEmpty(dto.Name))
            {
                // Check for duplicate
                var exists = await _context.Folders
                    .AnyAsync(f => f.Name == dto.Name &&
                        f.ParentFolderId == folder.ParentFolderId &&
                        f.Id != id &&
                        !f.IsDeleted);

                if (exists)
                    throw new InvalidOperationException("A folder with this name already exists");

                folder.Name = dto.Name;

                // Update path for this folder and all subfolders
                await UpdateFolderPathsAsync(folder);
            }

            if (dto.Description != null)
                folder.Description = dto.Description;

            folder.ModifiedAt = DateTime.UtcNow;
            folder.ModifiedBy = userId.ToString();

            await _context.SaveChangesAsync();

            // Audit log
            await _auditService.LogActionAsync("Update", "Folder", folder.Id, userId,
                $"Updated folder: {folder.Name}");

            return await GetFolderByIdAsync(id, userId);
        }

        public async Task DeleteFolderAsync(Guid id, Guid userId)
        {
            var folder = await _context.Folders
                .Include(f => f.Documents)
                .Include(f => f.SubFolders)
                .FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted);

            if (folder == null)
                throw new KeyNotFoundException("Folder not found");

            if (folder.IsSystemFolder)
                throw new InvalidOperationException("Cannot delete system folders");

            // Check permissions
            var hasAccess = await _permissionService.CanAccessFolderAsync(id, userId, "Delete");
            if (!hasAccess && folder.OwnerId != userId)
                throw new UnauthorizedAccessException("You don't have permission to delete this folder");

            // Check if folder is empty
            if (folder.Documents.Any(d => !d.IsDeleted) || folder.SubFolders.Any(sf => !sf.IsDeleted))
                throw new InvalidOperationException("Cannot delete folder with contents. Please delete all documents and subfolders first.");

            // Soft delete
            folder.IsDeleted = true;
            folder.DeletedAt = DateTime.UtcNow;
            folder.DeletedBy = userId.ToString();

            await _context.SaveChangesAsync();

            // Audit log
            await _auditService.LogActionAsync("Delete", "Folder", folder.Id, userId,
                $"Deleted folder: {folder.Name}");
        }

        public async Task<string> GetFolderPathAsync(Guid folderId)
        {
            var folder = await _context.Folders.FindAsync(folderId);
            return folder?.Path ?? "/";
        }

        private FolderTreeDTO BuildFolderTree(Folder folder, List<Folder> allFolders)
        {
            var children = allFolders
                .Where(f => f.ParentFolderId == folder.Id)
                .Select(f => BuildFolderTree(f, allFolders))
                .ToList();

            return new FolderTreeDTO
            {
                Id = folder.Id,
                Name = folder.Name,
                Path = folder.Path,
                Level = folder.Level,
                DocumentCount = folder.Documents?.Count ?? 0,
                Children = children
            };
        }

        private async Task UpdateFolderPathsAsync(Folder folder)
        {
            var newPath = folder.ParentFolderId.HasValue
                ? await GetFolderPathAsync(folder.ParentFolderId.Value) + folder.Name + "/"
                : $"/{folder.Name}/";

            folder.Path = newPath;

            // Update all subfolders recursively
            var subFolders = await _context.Folders
                .Where(f => f.ParentFolderId == folder.Id && !f.IsDeleted)
                .ToListAsync();

            foreach (var subFolder in subFolders)
            {
                await UpdateFolderPathsAsync(subFolder);
            }
        }
    }
}
```

### AuthService

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Backend.Data;
using Backend.Models;
using Backend.Models.DTO;
using Backend.Services.Interfaces;

namespace Backend.Services
{
    public class AuthService : IAuthService
    {
        private readonly EdmDbContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            EdmDbContext context,
            IMapper mapper,
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _context = context;
            _mapper = mapper;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<LoginResponseDTO> LoginAsync(LoginDTO dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == dto.Username && u.IsActive && !u.IsDeleted);

            if (user == null)
                return null;

            // Verify password
            if (!VerifyPassword(dto.Password, user.PasswordHash))
                return null;

            // Update last login
            await UpdateLastLoginAsync(user.Id);

            // Generate token
            var token = GenerateJwtToken(user);
            var expiresAt = DateTime.UtcNow.AddHours(8);

            return new LoginResponseDTO
            {
                Token = token,
                ExpiresAt = expiresAt,
                User = _mapper.Map<UserDTO>(user)
            };
        }

        public async Task<UserDTO> GetUserByIdAsync(Guid id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

            return _mapper.Map<UserDTO>(user);
        }

        public async Task<UserDTO> CreateUserAsync(CreateUserDTO dto)
        {
            // Check if username exists
            var exists = await _context.Users
                .AnyAsync(u => u.Username == dto.Username && !u.IsDeleted);

            if (exists)
                throw new InvalidOperationException("Username already exists");

            // Check if email exists
            exists = await _context.Users
                .AnyAsync(u => u.Email == dto.Email && !u.IsDeleted);

            if (exists)
                throw new InvalidOperationException("Email already exists");

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = HashPassword(dto.Password),
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Department = dto.Department,
                Role = dto.Role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return _mapper.Map<UserDTO>(user);
        }

        public async Task UpdateLastLoginAsync(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ValidatePasswordAsync(string username, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && !u.IsDeleted);

            if (user == null)
                return false;

            return VerifyPassword(password, user.PasswordHash);
        }

        public string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("userId", user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("fullName", $"{user.FirstName} {user.LastName}")
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private bool VerifyPassword(string password, string hash)
        {
            var passwordHash = HashPassword(password);
            return passwordHash == hash;
        }
    }
}
```

## Service Registration (Program.cs)

```csharp
// Add services to DI container
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IFolderService, FolderService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IWorkflowService, WorkflowService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IAuditService, AuditService>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });
```

## AutoMapper Profile

```csharp
using AutoMapper;
using Backend.Models;
using Backend.Models.DTO;

namespace Backend.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDTO>()
                .ForMember(dest => dest.FullName,
                    opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));

            CreateMap<DocumentVersion, DocumentVersionDTO>();
            CreateMap<Permission, PermissionDTO>();
            CreateMap<Tag, TagDTO>();
            CreateMap<Comment, CommentDTO>();
            CreateMap<Workflow, WorkflowDTO>();
            CreateMap<WorkflowStep, WorkflowStepDTO>();
        }
    }
}
```

## Configuration (appsettings.json)

```json
{
  "Jwt": {
    "Key": "YourSuperSecretKeyHere_MinimumLength32Characters!",
    "Issuer": "EDMSystem",
    "Audience": "EDMUsers"
  },
  "FileStorage": {
    "BasePath": "C:\\EDMStorage\\Documents",
    "MaxFileSizeBytes": 104857600
  }
}
```

---

**Document Version**: 1.0  
**Last Updated**: November 25, 2025  
**Author**: EDM Project Team  
**Status**: Design Complete - Ready for Implementation

## Next Steps

1. **Implement remaining services**: PermissionService, WorkflowService, FileStorageService, AuditService
2. **Add AutoMapper mappings** for all DTOs
3. **Configure JWT authentication** in Program.cs
4. **Implement file storage** with proper directory structure
5. **Add comprehensive unit tests** for all service methods
6. **Implement caching strategy** for frequently accessed data
7. **Add background jobs** with Hangfire for cleanup tasks
