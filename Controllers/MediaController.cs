using eShopServer.DTOs;
using eShopServer.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace eShopServer.Controllers;

/// <summary>
/// Admin endpoints for managing media assets (upload, browse, link, delete).
/// In production, protect with authentication/authorization.
/// </summary>
[ApiController]
[Route("api/admin/media")]
public class MediaController : ControllerBase
{
    private readonly IMediaService _mediaService;

    public MediaController(IMediaService mediaService)
    {
        _mediaService = mediaService;
    }

    // ════════════════════════════════════════════════════════════════
    //  Upload
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Upload a new media file. Send as multipart/form-data.
    /// </summary>
    [HttpPost("upload")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB
    public async Task<IActionResult> Upload(
        IFormFile file,
        [FromForm] string? altText,
        [FromForm] string? title,
        [FromForm] string category = "general")
    {
        try
        {
            var result = await _mediaService.UploadAsync(file, altText, title, category);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ════════════════════════════════════════════════════════════════
    //  Browse & Get
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Browse all media assets with optional search and category filter.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? category,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _mediaService.GetAllAsync(search, category, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Get a single media asset with all its usage details.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediaService.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    // ════════════════════════════════════════════════════════════════
    //  Update Metadata
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Update metadata (alt text, title, category) for an existing asset.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateMetadata(
        int id, [FromBody] UpdateMediaMetadataRequest request)
    {
        try
        {
            var result = await _mediaService.UpdateMetadataAsync(id, request);
            return result is null ? NotFound() : Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ════════════════════════════════════════════════════════════════
    //  Delete
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Delete a media asset (file + database record + all usage links).
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
        => await _mediaService.DeleteAsync(id) ? NoContent() : NotFound();

    // ════════════════════════════════════════════════════════════════
    //  Usage Tracking
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Link a media asset to an entity (e.g., link image #5 to CarouselSlide #3).
    /// </summary>
    [HttpPost("link")]
    public async Task<IActionResult> Link([FromBody] LinkMediaRequest request)
    {
        var result = await _mediaService.LinkAsync(request);
        return result is null
            ? NotFound(new { error = "Media asset not found." })
            : Ok(result);
    }

    /// <summary>
    /// Remove a usage link by its ID.
    /// </summary>
    [HttpDelete("link/{usageId:int}")]
    public async Task<IActionResult> Unlink(int usageId)
        => await _mediaService.UnlinkAsync(usageId) ? NoContent() : NotFound();

    /// <summary>
    /// Get all usages (where used) for a specific media asset.
    /// </summary>
    [HttpGet("{id:int}/usages")]
    public async Task<IActionResult> GetUsages(int id)
    {
        var result = await _mediaService.GetUsagesForAssetAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Get all media assets linked to a specific entity.
    /// Example: GET /api/admin/media/entity/CarouselSlide/3
    /// </summary>
    [HttpGet("entity/{entityType}/{entityId:int}")]
    public async Task<IActionResult> GetMediaForEntity(string entityType, int entityId)
    {
        var result = await _mediaService.GetMediaForEntityAsync(entityType, entityId);
        return Ok(result);
    }
}
