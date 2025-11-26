# EDM System - API Controllers & DTOs

## API Design Overview

This document defines the RESTful API endpoints, controllers, and Data Transfer Objects (DTOs) for the EDM system. The API follows REST best practices with clear resource naming, proper HTTP methods, and consistent response formats.

## API Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    API Layer Structure                       │
│                                                              │
│  Controllers  →  DTOs  →  Services  →  Repositories         │
│      ↓           ↓         ↓              ↓                 │
│  Routing    Validation  Business      Data Access           │
│  HTTP       Mapping     Logic         EF Core               │
└─────────────────────────────────────────────────────────────┘
```

## Base DTOs & Common Models

### BaseDTO

```csharp
namespace Backend.Models.DTO.Common
{
    public abstract class BaseDTO
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string ModifiedBy { get; set; }
    }
}
```

### ApiResponse

```csharp
namespace Backend.Models.DTO.Common
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public List<string> Errors { get; set; }

        public static ApiResponse<T> SuccessResponse(T data, string message = "Success")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                Errors = new List<string>()
            };
        }

        public static ApiResponse<T> ErrorResponse(string message, List<string> errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }
    }
}
```

### PagedResponse

```csharp
namespace Backend.Models.DTO.Common
{
    public class PagedResponse<T>
    {
        public List<T> Items { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPrevious => PageNumber > 1;
        public bool HasNext => PageNumber < TotalPages;
    }
}
```

### PaginationFilter

```csharp
using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTO.Common
{
    public class PaginationFilter
    {
        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 10;

        public string SearchTerm { get; set; }
        public string SortBy { get; set; }
        public bool SortDescending { get; set; } = false;
    }
}
```

## Document DTOs

### DocumentDTO

```csharp
using System.ComponentModel.DataAnnotations;
using Backend.Models.DTO.Common;

namespace Backend.Models.DTO
{
    public class DocumentDTO : BaseDTO
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public Guid FolderId { get; set; }
        public string FolderPath { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public string MimeType { get; set; }
        public string FileExtension { get; set; }
        public int CurrentVersion { get; set; }
        public string Status { get; set; }
        public Guid OwnerId { get; set; }
        public string OwnerName { get; set; }
        public bool IsPublic { get; set; }
        public List<string> Tags { get; set; }
        public int ViewCount { get; set; }
        public int DownloadCount { get; set; }
        public DateTime? ExpirationDate { get; set; }
    }
}
```

### CreateDocumentDTO

```csharp
using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTO
{
    public class CreateDocumentDTO
    {
        [Required]
        [MaxLength(500)]
        public string Title { get; set; }

        [MaxLength(2000)]
        public string Description { get; set; }

        [Required]
        public Guid FolderId { get; set; }

        [Required]
        public IFormFile File { get; set; }

        public List<string> Tags { get; set; }
        public bool IsPublic { get; set; } = false;
        public string CustomMetadata { get; set; }
        public DateTime? ExpirationDate { get; set; }
    }
}
```

### UpdateDocumentDTO

```csharp
using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTO
{
    public class UpdateDocumentDTO
    {
        [MaxLength(500)]
        public string Title { get; set; }

        [MaxLength(2000)]
        public string Description { get; set; }

        public Guid? FolderId { get; set; }
        public List<string> Tags { get; set; }
        public bool? IsPublic { get; set; }
        public string CustomMetadata { get; set; }
        public DateTime? ExpirationDate { get; set; }
    }
}
```

### DocumentVersionDTO

```csharp
namespace Backend.Models.DTO
{
    public class DocumentVersionDTO
    {
        public Guid Id { get; set; }
        public int VersionNumber { get; set; }
        public long FileSize { get; set; }
        public string VersionComment { get; set; }
        public bool IsCurrentVersion { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
    }
}
```

### DocumentSearchFilter

```csharp
using Backend.Models.DTO.Common;

namespace Backend.Models.DTO
{
    public class DocumentSearchFilter : PaginationFilter
    {
        public Guid? FolderId { get; set; }
        public List<string> Tags { get; set; }
        public string Status { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public string FileExtension { get; set; }
        public Guid? OwnerId { get; set; }
    }
}
```

## Folder DTOs

### FolderDTO

```csharp
using Backend.Models.DTO.Common;

namespace Backend.Models.DTO
{
    public class FolderDTO : BaseDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid? ParentFolderId { get; set; }
        public string Path { get; set; }
        public int Level { get; set; }
        public bool IsSystemFolder { get; set; }
        public Guid OwnerId { get; set; }
        public string OwnerName { get; set; }
        public int DocumentCount { get; set; }
        public int SubFolderCount { get; set; }
    }
}
```

### CreateFolderDTO

```csharp
using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTO
{
    public class CreateFolderDTO
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        public Guid? ParentFolderId { get; set; }
    }
}
```

### FolderTreeDTO

```csharp
namespace Backend.Models.DTO
{
    public class FolderTreeDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public int Level { get; set; }
        public int DocumentCount { get; set; }
        public List<FolderTreeDTO> Children { get; set; }
    }
}
```

## User DTOs

### UserDTO

```csharp
using Backend.Models.DTO.Common;

namespace Backend.Models.DTO
{
    public class UserDTO : BaseDTO
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string Department { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }
}
```

### CreateUserDTO

```csharp
using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTO
{
    public class CreateUserDTO
    {
        [Required]
        [MaxLength(100)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; }

        [Required]
        [MinLength(8)]
        public string Password { get; set; }

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }

        [MaxLength(100)]
        public string Department { get; set; }

        [Required]
        public string Role { get; set; }
    }
}
```

### LoginDTO

```csharp
using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTO
{
    public class LoginDTO
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
```

### LoginResponseDTO

```csharp
namespace Backend.Models.DTO
{
    public class LoginResponseDTO
    {
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
        public UserDTO User { get; set; }
    }
}
```

## Permission DTOs

### PermissionDTO

```csharp
namespace Backend.Models.DTO
{
    public class PermissionDTO
    {
        public Guid Id { get; set; }
        public Guid? FolderId { get; set; }
        public Guid? DocumentId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string PermissionType { get; set; }
        public bool IsInherited { get; set; }
        public DateTime GrantedAt { get; set; }
        public string GrantedBy { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}
```

### GrantPermissionDTO

```csharp
using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTO
{
    public class GrantPermissionDTO
    {
        public Guid? FolderId { get; set; }
        public Guid? DocumentId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public string PermissionType { get; set; } // Read, Write, Delete, Share, Admin

        public DateTime? ExpiresAt { get; set; }
    }
}
```

## Workflow DTOs

### WorkflowDTO

```csharp
using Backend.Models.DTO.Common;

namespace Backend.Models.DTO
{
    public class WorkflowDTO : BaseDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid DocumentId { get; set; }
        public string DocumentTitle { get; set; }
        public string WorkflowType { get; set; }
        public string Status { get; set; }
        public int CurrentStepOrder { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string CompletedBy { get; set; }
        public List<WorkflowStepDTO> Steps { get; set; }
    }
}
```

### CreateWorkflowDTO

```csharp
using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTO
{
    public class CreateWorkflowDTO
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        [Required]
        public Guid DocumentId { get; set; }

        [Required]
        public string WorkflowType { get; set; } // Approval, Review, Publishing

        public DateTime? DueDate { get; set; }

        [Required]
        [MinLength(1)]
        public List<CreateWorkflowStepDTO> Steps { get; set; }
    }
}
```

### WorkflowStepDTO

```csharp
namespace Backend.Models.DTO
{
    public class WorkflowStepDTO
    {
        public Guid Id { get; set; }
        public int StepOrder { get; set; }
        public string StepName { get; set; }
        public Guid AssignedToUserId { get; set; }
        public string AssignedToUserName { get; set; }
        public string Status { get; set; }
        public string Comment { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string CompletedBy { get; set; }
    }
}
```

### CreateWorkflowStepDTO

```csharp
using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTO
{
    public class CreateWorkflowStepDTO
    {
        [Required]
        public int StepOrder { get; set; }

        [Required]
        [MaxLength(255)]
        public string StepName { get; set; }

        [Required]
        public Guid AssignedToUserId { get; set; }

        public DateTime? DueDate { get; set; }
    }
}
```

### CompleteWorkflowStepDTO

```csharp
using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTO
{
    public class CompleteWorkflowStepDTO
    {
        [Required]
        public string Status { get; set; } // Approved, Rejected

        [MaxLength(1000)]
        public string Comment { get; set; }
    }
}
```

## Comment DTOs

### CommentDTO

```csharp
using Backend.Models.DTO.Common;

namespace Backend.Models.DTO
{
    public class CommentDTO : BaseDTO
    {
        public Guid DocumentId { get; set; }
        public Guid? ParentCommentId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string Text { get; set; }
        public bool IsResolved { get; set; }
        public List<CommentDTO> Replies { get; set; }
    }
}
```

### CreateCommentDTO

```csharp
using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTO
{
    public class CreateCommentDTO
    {
        [Required]
        public Guid DocumentId { get; set; }

        public Guid? ParentCommentId { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Text { get; set; }
    }
}
```

## Tag DTOs

### TagDTO

```csharp
using Backend.Models.DTO.Common;

namespace Backend.Models.DTO
{
    public class TagDTO : BaseDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Color { get; set; }
        public int UsageCount { get; set; }
    }
}
```

### CreateTagDTO

```csharp
using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTO
{
    public class CreateTagDTO
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [RegularExpression(@"^#[0-9A-Fa-f]{6}$")]
        public string Color { get; set; }
    }
}
```

## API Controllers

### 1. DocumentsController

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Backend.Models.DTO;
using Backend.Models.DTO.Common;
using Backend.Services.Interfaces;

namespace Backend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly ILogger<DocumentsController> _logger;

        public DocumentsController(
            IDocumentService documentService,
            ILogger<DocumentsController> logger)
        {
            _documentService = documentService;
            _logger = logger;
        }

        /// <summary>
        /// Get paginated list of documents with filtering
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<DocumentDTO>>>> GetDocuments(
            [FromQuery] DocumentSearchFilter filter)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _documentService.GetDocumentsAsync(filter, userId);
                return Ok(ApiResponse<PagedResponse<DocumentDTO>>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting documents");
                return StatusCode(500, ApiResponse<PagedResponse<DocumentDTO>>.ErrorResponse("Error retrieving documents"));
            }
        }

        /// <summary>
        /// Get document by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<DocumentDTO>>> GetDocument(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var document = await _documentService.GetDocumentByIdAsync(id, userId);

                if (document == null)
                    return NotFound(ApiResponse<DocumentDTO>.ErrorResponse("Document not found"));

                return Ok(ApiResponse<DocumentDTO>.SuccessResponse(document));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting document {DocumentId}", id);
                return StatusCode(500, ApiResponse<DocumentDTO>.ErrorResponse("Error retrieving document"));
            }
        }

        /// <summary>
        /// Upload new document
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<DocumentDTO>>> CreateDocument(
            [FromForm] CreateDocumentDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<DocumentDTO>.ErrorResponse("Invalid input", GetModelStateErrors()));

                var userId = GetCurrentUserId();
                var document = await _documentService.CreateDocumentAsync(dto, userId);

                return CreatedAtAction(
                    nameof(GetDocument),
                    new { id = document.Id },
                    ApiResponse<DocumentDTO>.SuccessResponse(document, "Document uploaded successfully"));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating document");
                return StatusCode(500, ApiResponse<DocumentDTO>.ErrorResponse("Error uploading document"));
            }
        }

        /// <summary>
        /// Update document metadata
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<DocumentDTO>>> UpdateDocument(
            Guid id,
            [FromBody] UpdateDocumentDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<DocumentDTO>.ErrorResponse("Invalid input", GetModelStateErrors()));

                var userId = GetCurrentUserId();
                var document = await _documentService.UpdateDocumentAsync(id, dto, userId);

                return Ok(ApiResponse<DocumentDTO>.SuccessResponse(document, "Document updated successfully"));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(ApiResponse<DocumentDTO>.ErrorResponse("Document not found"));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating document {DocumentId}", id);
                return StatusCode(500, ApiResponse<DocumentDTO>.ErrorResponse("Error updating document"));
            }
        }

        /// <summary>
        /// Delete document (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteDocument(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _documentService.DeleteDocumentAsync(id, userId);

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Document deleted successfully"));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse("Document not found"));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document {DocumentId}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResponse("Error deleting document"));
            }
        }

        /// <summary>
        /// Download document file
        /// </summary>
        [HttpGet("{id}/download")]
        public async Task<IActionResult> DownloadDocument(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var (fileStream, fileName, mimeType) = await _documentService.DownloadDocumentAsync(id, userId);

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
        /// Get document versions
        /// </summary>
        [HttpGet("{id}/versions")]
        public async Task<ActionResult<ApiResponse<List<DocumentVersionDTO>>>> GetDocumentVersions(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var versions = await _documentService.GetDocumentVersionsAsync(id, userId);

                return Ok(ApiResponse<List<DocumentVersionDTO>>.SuccessResponse(versions));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting versions for document {DocumentId}", id);
                return StatusCode(500, ApiResponse<List<DocumentVersionDTO>>.ErrorResponse("Error retrieving versions"));
            }
        }

        /// <summary>
        /// Upload new version of document
        /// </summary>
        [HttpPost("{id}/versions")]
        public async Task<ActionResult<ApiResponse<DocumentVersionDTO>>> CreateDocumentVersion(
            Guid id,
            [FromForm] IFormFile file,
            [FromForm] string comment)
        {
            try
            {
                var userId = GetCurrentUserId();
                var version = await _documentService.CreateDocumentVersionAsync(id, file, comment, userId);

                return Ok(ApiResponse<DocumentVersionDTO>.SuccessResponse(version, "New version uploaded successfully"));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating version for document {DocumentId}", id);
                return StatusCode(500, ApiResponse<DocumentVersionDTO>.ErrorResponse("Error uploading version"));
            }
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException());
        }

        private List<string> GetModelStateErrors()
        {
            return ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
        }
    }
}
```

### 2. FoldersController

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Backend.Models.DTO;
using Backend.Models.DTO.Common;
using Backend.Services.Interfaces;

namespace Backend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FoldersController : ControllerBase
    {
        private readonly IFolderService _folderService;
        private readonly ILogger<FoldersController> _logger;

        public FoldersController(
            IFolderService folderService,
            ILogger<FoldersController> logger)
        {
            _folderService = folderService;
            _logger = logger;
        }

        /// <summary>
        /// Get folder tree structure
        /// </summary>
        [HttpGet("tree")]
        public async Task<ActionResult<ApiResponse<List<FolderTreeDTO>>>> GetFolderTree()
        {
            try
            {
                var userId = GetCurrentUserId();
                var tree = await _folderService.GetFolderTreeAsync(userId);

                return Ok(ApiResponse<List<FolderTreeDTO>>.SuccessResponse(tree));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting folder tree");
                return StatusCode(500, ApiResponse<List<FolderTreeDTO>>.ErrorResponse("Error retrieving folder tree"));
            }
        }

        /// <summary>
        /// Get folder by ID with contents
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<FolderDTO>>> GetFolder(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var folder = await _folderService.GetFolderByIdAsync(id, userId);

                if (folder == null)
                    return NotFound(ApiResponse<FolderDTO>.ErrorResponse("Folder not found"));

                return Ok(ApiResponse<FolderDTO>.SuccessResponse(folder));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting folder {FolderId}", id);
                return StatusCode(500, ApiResponse<FolderDTO>.ErrorResponse("Error retrieving folder"));
            }
        }

        /// <summary>
        /// Create new folder
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<FolderDTO>>> CreateFolder(
            [FromBody] CreateFolderDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<FolderDTO>.ErrorResponse("Invalid input"));

                var userId = GetCurrentUserId();
                var folder = await _folderService.CreateFolderAsync(dto, userId);

                return CreatedAtAction(
                    nameof(GetFolder),
                    new { id = folder.Id },
                    ApiResponse<FolderDTO>.SuccessResponse(folder, "Folder created successfully"));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating folder");
                return StatusCode(500, ApiResponse<FolderDTO>.ErrorResponse("Error creating folder"));
            }
        }

        /// <summary>
        /// Delete folder
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteFolder(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _folderService.DeleteFolderAsync(id, userId);

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Folder deleted successfully"));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse("Folder not found"));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting folder {FolderId}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResponse("Error deleting folder"));
            }
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException());
        }
    }
}
```

### 3. AuthController

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Backend.Models.DTO;
using Backend.Models.DTO.Common;
using Backend.Services.Interfaces;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthService authService,
            ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// User login
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<LoginResponseDTO>>> Login(
            [FromBody] LoginDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<LoginResponseDTO>.ErrorResponse("Invalid input"));

                var result = await _authService.LoginAsync(dto);

                if (result == null)
                    return Unauthorized(ApiResponse<LoginResponseDTO>.ErrorResponse("Invalid credentials"));

                return Ok(ApiResponse<LoginResponseDTO>.SuccessResponse(result, "Login successful"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {Username}", dto.Username);
                return StatusCode(500, ApiResponse<LoginResponseDTO>.ErrorResponse("Login error"));
            }
        }

        /// <summary>
        /// Get current user profile
        /// </summary>
        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<ApiResponse<UserDTO>>> GetCurrentUser()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst("userId")?.Value);
                var user = await _authService.GetUserByIdAsync(userId);

                return Ok(ApiResponse<UserDTO>.SuccessResponse(user));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user");
                return StatusCode(500, ApiResponse<UserDTO>.ErrorResponse("Error retrieving user"));
            }
        }
    }
}
```

## API Endpoints Summary

### Documents

- `GET /api/documents` - List documents (paginated, filtered)
- `GET /api/documents/{id}` - Get document by ID
- `POST /api/documents` - Upload document
- `PUT /api/documents/{id}` - Update document metadata
- `DELETE /api/documents/{id}` - Delete document
- `GET /api/documents/{id}/download` - Download file
- `GET /api/documents/{id}/versions` - List versions
- `POST /api/documents/{id}/versions` - Upload new version

### Folders

- `GET /api/folders/tree` - Get folder tree
- `GET /api/folders/{id}` - Get folder details
- `POST /api/folders` - Create folder
- `PUT /api/folders/{id}` - Update folder
- `DELETE /api/folders/{id}` - Delete folder

### Users (Admin only)

- `GET /api/users` - List users
- `GET /api/users/{id}` - Get user by ID
- `POST /api/users` - Create user
- `PUT /api/users/{id}` - Update user
- `DELETE /api/users/{id}` - Deactivate user

### Auth

- `POST /api/auth/login` - User login
- `GET /api/auth/me` - Get current user

### Permissions

- `GET /api/permissions/document/{id}` - Get document permissions
- `POST /api/permissions` - Grant permission
- `DELETE /api/permissions/{id}` - Revoke permission

### Workflows

- `GET /api/workflows` - List workflows
- `GET /api/workflows/{id}` - Get workflow details
- `POST /api/workflows` - Create workflow
- `PUT /api/workflows/{id}/steps/{stepId}` - Complete workflow step

### Comments

- `GET /api/documents/{id}/comments` - Get comments
- `POST /api/comments` - Add comment
- `PUT /api/comments/{id}` - Update comment
- `DELETE /api/comments/{id}` - Delete comment

### Tags

- `GET /api/tags` - List tags
- `POST /api/tags` - Create tag
- `DELETE /api/tags/{id}` - Delete tag

---

**Document Version**: 1.0  
**Last Updated**: November 25, 2025  
**Author**: EDM Project Team  
**Status**: Design Complete - Ready for Implementation
