using Backend.Data;
using Backend.Models.DTO.Common;
using Backend.Models.DTO.Tags;
using Backend.Models.Document;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Backend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TagsController : ControllerBase
{
    private readonly DocumentDbContext _context;

    public TagsController(DocumentDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<TagDTO>>>> GetTags()
    {
        var tags = await _context.Tags
            .Include(t => t.DocumentTags)
            .Select(t => new TagDTO
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                Color = t.Color,
                DocumentCount = t.DocumentTags.Count
            })
            .ToListAsync();

        return Ok(ApiResponse<List<TagDTO>>.SuccessResponse(tags));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<TagDTO>>> GetTag(Guid id)
    {
        var tag = await _context.Tags
            .Include(t => t.DocumentTags)
            .Where(t => t.Id == id)
            .Select(t => new TagDTO
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                Color = t.Color,
                DocumentCount = t.DocumentTags.Count
            })
            .FirstOrDefaultAsync();

        if (tag == null)
            return NotFound(ApiResponse<TagDTO>.ErrorResponse("Tag not found"));

        return Ok(ApiResponse<TagDTO>.SuccessResponse(tag));
    }

    [Authorize(Roles = "SystemAdmin,Admin")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<TagDTO>>> CreateTag([FromBody] CreateTagRequest request)
    {
        var username = User.Identity?.Name ?? "";

        // Check if tag name already exists
        if (await _context.Tags.AnyAsync(t => t.Name == request.Name))
            return BadRequest(ApiResponse<TagDTO>.ErrorResponse("Tag with this name already exists"));

        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Color = request.Color,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = username
        };

        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();

        var tagDto = new TagDTO
        {
            Id = tag.Id,
            Name = tag.Name,
            Description = tag.Description,
            Color = tag.Color,
            DocumentCount = 0
        };

        return CreatedAtAction(nameof(GetTag), new { id = tag.Id },
            ApiResponse<TagDTO>.SuccessResponse(tagDto, "Tag created successfully"));
    }

    [Authorize(Roles = "SystemAdmin,Admin")]
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<TagDTO>>> UpdateTag(Guid id, [FromBody] CreateTagRequest request)
    {
        var username = User.Identity?.Name ?? "";

        var tag = await _context.Tags.FindAsync(id);
        if (tag == null || tag.IsDeleted)
            return NotFound(ApiResponse<TagDTO>.ErrorResponse("Tag not found"));

        // Check if new name conflicts with existing tag
        if (await _context.Tags.AnyAsync(t => t.Name == request.Name && t.Id != id))
            return BadRequest(ApiResponse<TagDTO>.ErrorResponse("Tag with this name already exists"));

        tag.Name = request.Name;
        tag.Description = request.Description;
        tag.Color = request.Color;
        tag.ModifiedAt = DateTime.UtcNow;
        tag.ModifiedBy = username;

        await _context.SaveChangesAsync();

        var result = await GetTag(id);
        return Ok(ApiResponse<TagDTO>.SuccessResponse(result.Value?.Data!, "Tag updated successfully"));
    }

    [Authorize(Roles = "SystemAdmin,Admin")]
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteTag(Guid id)
    {
        var username = User.Identity?.Name ?? "";

        var tag = await _context.Tags.FindAsync(id);
        if (tag == null || tag.IsDeleted)
            return NotFound(ApiResponse<bool>.ErrorResponse("Tag not found"));

        // Soft delete
        tag.IsDeleted = true;
        tag.DeletedAt = DateTime.UtcNow;
        tag.DeletedBy = username;

        await _context.SaveChangesAsync();

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Tag deleted successfully"));
    }
}
