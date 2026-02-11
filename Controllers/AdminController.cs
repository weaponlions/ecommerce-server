using eShopServer.DTOs;
using eShopServer.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace eShopServer.Controllers;

/// <summary>
/// Admin controller for managing all dashboard content.
/// In production, protect these endpoints with authentication/authorization.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    // ════════════════════════════════════════════════════════════════
    //  Dashboard Sections
    // ════════════════════════════════════════════════════════════════

    [HttpGet("sections")]
    public async Task<IActionResult> GetAllSections()
        => Ok(await _adminService.GetAllSectionsAsync());

    [HttpPut("sections/{id:int}")]
    public async Task<IActionResult> UpdateSection(int id, [FromBody] UpsertDashboardSectionRequest request)
    {
        var result = await _adminService.UpdateSectionAsync(id, request);
        return result is null ? NotFound() : Ok(result);
    }

    // ════════════════════════════════════════════════════════════════
    //  Navbar Links
    // ════════════════════════════════════════════════════════════════

    [HttpGet("navbar")]
    public async Task<IActionResult> GetAllNavbarLinks()
        => Ok(await _adminService.GetAllNavbarLinksAsync());

    [HttpPost("navbar")]
    public async Task<IActionResult> CreateNavbarLink([FromBody] UpsertNavbarLinkRequest request)
    {
        var result = await _adminService.CreateNavbarLinkAsync(request);
        return CreatedAtAction(nameof(GetAllNavbarLinks), new { id = result.Id }, result);
    }

    [HttpPut("navbar/{id:int}")]
    public async Task<IActionResult> UpdateNavbarLink(int id, [FromBody] UpsertNavbarLinkRequest request)
    {
        var result = await _adminService.UpdateNavbarLinkAsync(id, request);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("navbar/{id:int}")]
    public async Task<IActionResult> DeleteNavbarLink(int id)
        => await _adminService.DeleteNavbarLinkAsync(id) ? NoContent() : NotFound();

    // ════════════════════════════════════════════════════════════════
    //  Carousel Slides
    // ════════════════════════════════════════════════════════════════

    [HttpGet("carousel")]
    public async Task<IActionResult> GetAllCarouselSlides()
        => Ok(await _adminService.GetAllCarouselSlidesAsync());

    [HttpPost("carousel")]
    public async Task<IActionResult> CreateCarouselSlide([FromBody] UpsertCarouselSlideRequest request)
    {
        var result = await _adminService.CreateCarouselSlideAsync(request);
        return CreatedAtAction(nameof(GetAllCarouselSlides), new { id = result.Id }, result);
    }

    [HttpPut("carousel/{id:int}")]
    public async Task<IActionResult> UpdateCarouselSlide(int id, [FromBody] UpsertCarouselSlideRequest request)
    {
        var result = await _adminService.UpdateCarouselSlideAsync(id, request);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("carousel/{id:int}")]
    public async Task<IActionResult> DeleteCarouselSlide(int id)
        => await _adminService.DeleteCarouselSlideAsync(id) ? NoContent() : NotFound();

    // ════════════════════════════════════════════════════════════════
    //  Products — Managed via AdminProductsController (api/admin/products)
    // ════════════════════════════════════════════════════════════════

    // ════════════════════════════════════════════════════════════════
    //  Collections
    // ════════════════════════════════════════════════════════════════

    [HttpGet("collections")]
    public async Task<IActionResult> GetAllCollections()
        => Ok(await _adminService.GetAllCollectionsAsync());

    [HttpPost("collections")]
    public async Task<IActionResult> CreateCollection([FromBody] UpsertCollectionRequest request)
    {
        var result = await _adminService.CreateCollectionAsync(request);
        return CreatedAtAction(nameof(GetAllCollections), new { id = result.Id }, result);
    }

    [HttpPut("collections/{id:int}")]
    public async Task<IActionResult> UpdateCollection(int id, [FromBody] UpsertCollectionRequest request)
    {
        var result = await _adminService.UpdateCollectionAsync(id, request);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("collections/{id:int}")]
    public async Task<IActionResult> DeleteCollection(int id)
        => await _adminService.DeleteCollectionAsync(id) ? NoContent() : NotFound();

    // ════════════════════════════════════════════════════════════════
    //  Footer Links
    // ════════════════════════════════════════════════════════════════

    [HttpGet("footer-links")]
    public async Task<IActionResult> GetAllFooterLinks()
        => Ok(await _adminService.GetAllFooterLinksAsync());

    [HttpPost("footer-links")]
    public async Task<IActionResult> CreateFooterLink([FromBody] UpsertFooterLinkRequest request)
    {
        var result = await _adminService.CreateFooterLinkAsync(request);
        return CreatedAtAction(nameof(GetAllFooterLinks), new { id = result.Id }, result);
    }

    [HttpPut("footer-links/{id:int}")]
    public async Task<IActionResult> UpdateFooterLink(int id, [FromBody] UpsertFooterLinkRequest request)
    {
        var result = await _adminService.UpdateFooterLinkAsync(id, request);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("footer-links/{id:int}")]
    public async Task<IActionResult> DeleteFooterLink(int id)
        => await _adminService.DeleteFooterLinkAsync(id) ? NoContent() : NotFound();

    // ════════════════════════════════════════════════════════════════
    //  Social Icons
    // ════════════════════════════════════════════════════════════════

    [HttpGet("social-icons")]
    public async Task<IActionResult> GetAllSocialIcons()
        => Ok(await _adminService.GetAllSocialIconsAsync());

    [HttpPost("social-icons")]
    public async Task<IActionResult> CreateSocialIcon([FromBody] UpsertSocialIconRequest request)
    {
        var result = await _adminService.CreateSocialIconAsync(request);
        return CreatedAtAction(nameof(GetAllSocialIcons), new { id = result.Id }, result);
    }

    [HttpPut("social-icons/{id:int}")]
    public async Task<IActionResult> UpdateSocialIcon(int id, [FromBody] UpsertSocialIconRequest request)
    {
        var result = await _adminService.UpdateSocialIconAsync(id, request);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("social-icons/{id:int}")]
    public async Task<IActionResult> DeleteSocialIcon(int id)
        => await _adminService.DeleteSocialIconAsync(id) ? NoContent() : NotFound();
}
