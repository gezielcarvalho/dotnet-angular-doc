using System.Security.Claims;
using Backend.Data;
using Backend.Models.DTO.Common;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly DocumentDbContext _context;
    private readonly ILogger<AdminController> _logger;

    public AdminController(DocumentDbContext context, ILogger<AdminController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [Authorize(Roles = "Admin,SystemAdmin")]
    [HttpPost("run-personal-folder-migration")]
    public async Task<ActionResult<ApiResponse<int>>> RunPersonalFolderMigration()
    {
        // Double-check role for direct invocation and to return Forbid without relying on middleware
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        if (role != "Admin" && role != "SystemAdmin")
            return Forbid();
        try
        {
            var createdCount = await DbSeeder.EnsurePersonalFoldersForExistingUsers(_context);
            _logger.LogInformation("Personal folder migration created {Count} folders", createdCount);
            return Ok(ApiResponse<int>.SuccessResponse(createdCount, "Migration completed"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to run personal folder migration");
            return StatusCode(500, ApiResponse<int>.ErrorResponse("Migration failed", new List<string> { ex.Message }));
        }
    }
}
