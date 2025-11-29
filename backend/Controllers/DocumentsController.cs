using Backend.Data;
using Backend.Models.DTO.Common;
using Backend.Models.DTO.Documents;
using Backend.Models.Document;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Backend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly EdmDbContext _context;
    private readonly IPermissionService _permissionService;
    private readonly IFileStorageService _fileStorageService;

    public DocumentsController(
        EdmDbContext context,
        IPermissionService permissionService,
        IFileStorageService fileStorageService)
    {
        _context = context;
        _permissionService = permissionService;
        _fileStorageService = fileStorageService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResponse<DocumentDTO>>>> GetDocuments(
        [FromQuery] Guid? folderId,
        [FromQuery] PaginationFilter filter)
    {
        var userId = GetCurrentUserId();
        var query = _context.Documents.AsQueryable();

        if (folderId.HasValue)
        {
            if (!await _permissionService.CanAccessFolderAsync(userId, folderId.Value, "Read"))
                return Forbid();

            query = query.Where(d => d.FolderId == folderId.Value);
        }

        if (!string.IsNullOrEmpty(filter.SearchTerm))
        {
            query = query.Where(d => d.Title.Contains(filter.SearchTerm) ||
                                    (d.Description != null && d.Description.Contains(filter.SearchTerm)));
        }

        var totalCount = await query.CountAsync();

        var documents = await query
            .Include(d => d.Owner)
            .Include(d => d.Folder)
            .Include(d => d.DocumentTags)
                .ThenInclude(dt => dt.Tag)
            .OrderByDescending(d => d.CreatedAt)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(d => new DocumentDTO
            {
                Id = d.Id,
                Title = d.Title,
                Description = d.Description,
                FileName = d.FileName,
                FileExtension = d.FileExtension,
                FileSizeBytes = d.FileSizeBytes,
                Status = d.Status,
                CurrentVersion = d.CurrentVersion,
                FolderId = d.FolderId,
                FolderPath = d.Folder.Path,
                OwnerId = d.OwnerId,
                OwnerName = d.Owner.Username,
                CreatedAt = d.CreatedAt,
                ModifiedAt = d.ModifiedAt,
                Tags = d.DocumentTags.Select(dt => dt.Tag.Name).ToList(),
                HasActiveWorkflow = d.Workflows.Any(w => w.Status == "Pending")
            })
            .ToListAsync();

        // Filter by permissions
        var accessibleDocuments = new List<DocumentDTO>();
        foreach (var doc in documents)
        {
            if (await _permissionService.CanAccessDocumentAsync(userId, doc.Id, "Read"))
                accessibleDocuments.Add(doc);
        }

        var pagedResponse = new PagedResponse<DocumentDTO>
        {
            Items = accessibleDocuments,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
        };

        return Ok(ApiResponse<PagedResponse<DocumentDTO>>.SuccessResponse(pagedResponse));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<DocumentDTO>>> GetDocument(Guid id)
    {
        var userId = GetCurrentUserId();

        if (!await _permissionService.CanAccessDocumentAsync(userId, id, "Read"))
            return Forbid();

        var document = await _context.Documents
            .Include(d => d.Owner)
            .Include(d => d.Folder)
            .Include(d => d.DocumentTags)
                .ThenInclude(dt => dt.Tag)
            .Where(d => d.Id == id)
            .Select(d => new DocumentDTO
            {
                Id = d.Id,
                Title = d.Title,
                Description = d.Description,
                FileName = d.FileName,
                FileExtension = d.FileExtension,
                FileSizeBytes = d.FileSizeBytes,
                Status = d.Status,
                CurrentVersion = d.CurrentVersion,
                FolderId = d.FolderId,
                FolderPath = d.Folder.Path,
                OwnerId = d.OwnerId,
                OwnerName = d.Owner.Username,
                CreatedAt = d.CreatedAt,
                ModifiedAt = d.ModifiedAt,
                Tags = d.DocumentTags.Select(dt => dt.Tag.Name).ToList(),
                HasActiveWorkflow = d.Workflows.Any(w => w.Status == "Pending")
            })
            .FirstOrDefaultAsync();

        if (document == null)
            return NotFound(ApiResponse<DocumentDTO>.ErrorResponse("Document not found"));

        return Ok(ApiResponse<DocumentDTO>.SuccessResponse(document));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<DocumentDTO>>> CreateDocument(
        [FromForm] CreateDocumentRequest request)
    {
        var userId = GetCurrentUserId();
        var username = GetCurrentUsername();

        if (!await _permissionService.CanAccessFolderAsync(userId, request.FolderId, "Write"))
            return Forbid();

        if (request.File == null || request.File.Length == 0)
            return BadRequest(ApiResponse<DocumentDTO>.ErrorResponse("File is required"));

        var extension = _fileStorageService.GetFileExtension(request.File.FileName);
        if (!_fileStorageService.IsAllowedExtension(extension))
            return BadRequest(ApiResponse<DocumentDTO>.ErrorResponse($"File type {extension} is not allowed"));

        if (!_fileStorageService.IsFileSizeValid(request.File.Length))
            return BadRequest(ApiResponse<DocumentDTO>.ErrorResponse("File size exceeds maximum allowed size"));

        var documentId = Guid.NewGuid();

        // Save file
        string filePath;
        using (var stream = request.File.OpenReadStream())
        {
            filePath = await _fileStorageService.SaveFileAsync(stream, request.File.FileName, documentId, 1);
        }

        var document = new Document
        {
            Id = documentId,
            Title = request.Title,
            Description = request.Description,
            FileName = request.File.FileName,
            FileExtension = extension,
            FileSizeBytes = request.File.Length,
            FilePath = filePath,
            Status = "Draft",
            CurrentVersion = 1,
            FolderId = request.FolderId,
            OwnerId = userId,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = username
        };

        _context.Documents.Add(document);

        // Create first version
        var version = new DocumentVersion
        {
            Id = Guid.NewGuid(),
            DocumentId = documentId,
            VersionNumber = 1,
            FileName = request.File.FileName,
            FilePath = filePath,
            FileSizeBytes = request.File.Length,
            ChangeComment = "Initial version",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = username
        };

        _context.DocumentVersions.Add(version);

        // Add tags
        if (request.TagIds != null && request.TagIds.Any())
        {
            foreach (var tagId in request.TagIds)
            {
                _context.DocumentTags.Add(new DocumentTag
                {
                    DocumentId = documentId,
                    TagId = tagId
                });
            }
        }

        await _context.SaveChangesAsync();

        var result = await GetDocument(documentId);
        return CreatedAtAction(nameof(GetDocument), new { id = documentId },
            ApiResponse<DocumentDTO>.SuccessResponse(result.Value?.Data!, "Document created successfully"));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<DocumentDTO>>> UpdateDocument(Guid id, [FromBody] UpdateDocumentRequest request)
    {
        var userId = GetCurrentUserId();
        var username = GetCurrentUsername();

        if (!await _permissionService.CanAccessDocumentAsync(userId, id, "Write"))
            return Forbid();

        var document = await _context.Documents
            .Include(d => d.DocumentTags)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (document == null || document.IsDeleted)
            return NotFound(ApiResponse<DocumentDTO>.ErrorResponse("Document not found"));

        document.Title = request.Title;
        document.Description = request.Description;
        
        if (!string.IsNullOrEmpty(request.Status))
            document.Status = request.Status;

        document.ModifiedAt = DateTime.UtcNow;
        document.ModifiedBy = username;

        // Update tags
        if (request.TagIds != null)
        {
            _context.DocumentTags.RemoveRange(document.DocumentTags);
            foreach (var tagId in request.TagIds)
            {
                _context.DocumentTags.Add(new DocumentTag
                {
                    DocumentId = id,
                    TagId = tagId
                });
            }
        }

        await _context.SaveChangesAsync();

        var result = await GetDocument(id);
        return Ok(ApiResponse<DocumentDTO>.SuccessResponse(result.Value?.Data!, "Document updated successfully"));
    }

    [HttpPost("{id}/upload-version")]
    public async Task<ActionResult<ApiResponse<DocumentVersionDTO>>> UploadNewVersion(
        Guid id,
        [FromForm] UploadDocumentVersionRequest request)
    {
        var userId = GetCurrentUserId();
        var username = GetCurrentUsername();

        if (!await _permissionService.CanAccessDocumentAsync(userId, id, "Write"))
            return Forbid();

        var document = await _context.Documents.FindAsync(id);
        if (document == null || document.IsDeleted)
            return NotFound(ApiResponse<DocumentVersionDTO>.ErrorResponse("Document not found"));

        if (request.File == null || request.File.Length == 0)
            return BadRequest(ApiResponse<DocumentVersionDTO>.ErrorResponse("File is required"));

        var extension = _fileStorageService.GetFileExtension(request.File.FileName);
        if (!_fileStorageService.IsAllowedExtension(extension))
            return BadRequest(ApiResponse<DocumentVersionDTO>.ErrorResponse($"File type {extension} is not allowed"));

        if (!_fileStorageService.IsFileSizeValid(request.File.Length))
            return BadRequest(ApiResponse<DocumentVersionDTO>.ErrorResponse("File size exceeds maximum allowed size"));

        var newVersion = document.CurrentVersion + 1;

        // Save file
        string filePath;
        using (var stream = request.File.OpenReadStream())
        {
            filePath = await _fileStorageService.SaveFileAsync(stream, request.File.FileName, id, newVersion);
        }

        // Update document
        document.CurrentVersion = newVersion;
        document.FileName = request.File.FileName;
        document.FileExtension = extension;
        document.FileSizeBytes = request.File.Length;
        document.FilePath = filePath;
        document.ModifiedAt = DateTime.UtcNow;
        document.ModifiedBy = username;

        // Create version record
        var version = new DocumentVersion
        {
            Id = Guid.NewGuid(),
            DocumentId = id,
            VersionNumber = newVersion,
            FileName = request.File.FileName,
            FilePath = filePath,
            FileSizeBytes = request.File.Length,
            ChangeComment = request.ChangeComment ?? $"Version {newVersion}",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = username
        };

        _context.DocumentVersions.Add(version);
        await _context.SaveChangesAsync();

        var versionDto = new DocumentVersionDTO
        {
            Id = version.Id,
            VersionNumber = version.VersionNumber,
            FileName = version.FileName,
            FilePath = version.FilePath,
            FileSizeBytes = version.FileSizeBytes,
            ChangeComment = version.ChangeComment,
            CreatedAt = version.CreatedAt,
            CreatedBy = version.CreatedBy
        };

        return Ok(ApiResponse<DocumentVersionDTO>.SuccessResponse(versionDto, "New version uploaded successfully"));
    }

    [HttpGet("{id}/download")]
    public async Task<IActionResult> DownloadDocument(Guid id, [FromQuery] int? version)
    {
        var userId = GetCurrentUserId();

        if (!await _permissionService.CanAccessDocumentAsync(userId, id, "Read"))
            return Forbid();

        var document = await _context.Documents.FindAsync(id);
        if (document == null || document.IsDeleted)
            return NotFound();

        string filePath;
        string fileName;

        if (version.HasValue && version.Value != document.CurrentVersion)
        {
            var docVersion = await _context.DocumentVersions
                .FirstOrDefaultAsync(v => v.DocumentId == id && v.VersionNumber == version.Value);
            
            if (docVersion == null)
                return NotFound();

            filePath = docVersion.FilePath;
            fileName = docVersion.FileName;
        }
        else
        {
            filePath = document.FilePath;
            fileName = document.FileName;
        }

        var fileStream = await _fileStorageService.GetFileAsync(filePath);
        if (fileStream == null)
            return NotFound();

        return File(fileStream, "application/octet-stream", fileName);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteDocument(Guid id)
    {
        var userId = GetCurrentUserId();
        var username = GetCurrentUsername();

        if (!await _permissionService.CanAccessDocumentAsync(userId, id, "Admin"))
            return Forbid();

        var document = await _context.Documents.FindAsync(id);
        if (document == null || document.IsDeleted)
            return NotFound(ApiResponse<bool>.ErrorResponse("Document not found"));

        // Soft delete
        document.IsDeleted = true;
        document.DeletedAt = DateTime.UtcNow;
        document.DeletedBy = username;

        await _context.SaveChangesAsync();

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Document deleted successfully"));
    }

    [HttpGet("{id}/versions")]
    public async Task<ActionResult<ApiResponse<List<DocumentVersionDTO>>>> GetDocumentVersions(Guid id)
    {
        var userId = GetCurrentUserId();

        if (!await _permissionService.CanAccessDocumentAsync(userId, id, "Read"))
            return Forbid();

        var versions = await _context.DocumentVersions
            .Where(v => v.DocumentId == id)
            .OrderByDescending(v => v.VersionNumber)
            .Select(v => new DocumentVersionDTO
            {
                Id = v.Id,
                VersionNumber = v.VersionNumber,
                FileName = v.FileName,
                FilePath = v.FilePath,
                FileSizeBytes = v.FileSizeBytes,
                ChangeComment = v.ChangeComment,
                CreatedAt = v.CreatedAt,
                CreatedBy = v.CreatedBy
            })
            .ToListAsync();

        return Ok(ApiResponse<List<DocumentVersionDTO>>.SuccessResponse(versions));
    }

    private Guid GetCurrentUserId()
    {
        return Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
    }

    private string GetCurrentUsername()
    {
        return User.Identity?.Name ?? "";
    }
}
