using eShopServer.DTOs;
using eShopServer.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace eShopServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Returns the full dashboard with all visible sections in server-defined order.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetFullDashboard([FromQuery] string? userId)
    {
        var result = await _dashboardService.GetFullDashboardAsync(userId);
        return Ok(result);
    }

    /// <summary>
    /// Returns navbar links with nested children.
    /// </summary>
    [HttpGet("navbar")]
    public async Task<IActionResult> GetNavbar()
    {
        var result = await _dashboardService.GetNavbarAsync();
        return Ok(result);
    }

    /// <summary>
    /// Returns active carousel slides (respects scheduling).
    /// </summary>
    [HttpGet("carousel")]
    public async Task<IActionResult> GetCarousel()
    {
        var result = await _dashboardService.GetCarouselAsync();
        return Ok(result);
    }

    /// <summary>
    /// Returns top trending products (server-limited).
    /// </summary>
    [HttpGet("trending")]
    public async Task<IActionResult> GetTrending()
    {
        var result = await _dashboardService.GetTrendingAsync();
        return Ok(result);
    }

    /// <summary>
    /// Returns recently visited products for a specific user.
    /// </summary>
    [HttpGet("recently-visited/{userId}")]
    public async Task<IActionResult> GetRecentlyVisited(string userId)
    {
        var result = await _dashboardService.GetRecentlyVisitedAsync(userId);
        return Ok(result);
    }

    /// <summary>
    /// Tracks a user's product visit.
    /// </summary>
    [HttpPost("recently-visited")]
    public async Task<IActionResult> TrackVisit([FromBody] TrackVisitRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
            return BadRequest(new { error = "UserId is required." });

        var success = await _dashboardService.TrackVisitAsync(request);
        if (!success)
            return NotFound(new { error = "Product not found." });

        return Ok(new { message = "Visit tracked." });
    }

    /// <summary>
    /// Returns most visited collections (server-limited).
    /// </summary>
    [HttpGet("collections")]
    public async Task<IActionResult> GetCollections()
    {
        var result = await _dashboardService.GetCollectionsAsync();
        return Ok(result);
    }

    /// <summary>
    /// Returns footer link groups and social icons.
    /// </summary>
    [HttpGet("footer")]
    public async Task<IActionResult> GetFooter()
    {
        var result = await _dashboardService.GetFooterAsync();
        return Ok(result);
    }
}
