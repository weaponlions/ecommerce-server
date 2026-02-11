using eShopServer.DTOs;
using eShopServer.Models;

namespace eShopServer.Interfaces.Services;

/// <summary>
/// Service contract for all admin CRUD operations across dashboard entities.
/// </summary>
public interface IAdminService
{
    // ── Dashboard Sections ──
    Task<IEnumerable<DashboardSection>> GetAllSectionsAsync();
    Task<DashboardSection?> UpdateSectionAsync(int id, UpsertDashboardSectionRequest request);

    // ── Navbar Links ──
    Task<IEnumerable<NavbarLink>> GetAllNavbarLinksAsync();
    Task<NavbarLink> CreateNavbarLinkAsync(UpsertNavbarLinkRequest request);
    Task<NavbarLink?> UpdateNavbarLinkAsync(int id, UpsertNavbarLinkRequest request);
    Task<bool> DeleteNavbarLinkAsync(int id);

    // ── Carousel Slides ──
    Task<IEnumerable<CarouselSlide>> GetAllCarouselSlidesAsync();
    Task<CarouselSlide> CreateCarouselSlideAsync(UpsertCarouselSlideRequest request);
    Task<CarouselSlide?> UpdateCarouselSlideAsync(int id, UpsertCarouselSlideRequest request);
    Task<bool> DeleteCarouselSlideAsync(int id);

    // ── Products ──
    Task<IEnumerable<Product>> GetAllProductsAsync();
    Task<Product> CreateProductAsync(UpsertProductRequest request);
    Task<Product?> UpdateProductAsync(int id, UpsertProductRequest request);
    Task<bool> DeleteProductAsync(int id);

    // ── Collections ──
    Task<IEnumerable<Collection>> GetAllCollectionsAsync();
    Task<Collection> CreateCollectionAsync(UpsertCollectionRequest request);
    Task<Collection?> UpdateCollectionAsync(int id, UpsertCollectionRequest request);
    Task<bool> DeleteCollectionAsync(int id);

    // ── Footer Links ──
    Task<IEnumerable<FooterLink>> GetAllFooterLinksAsync();
    Task<FooterLink> CreateFooterLinkAsync(UpsertFooterLinkRequest request);
    Task<FooterLink?> UpdateFooterLinkAsync(int id, UpsertFooterLinkRequest request);
    Task<bool> DeleteFooterLinkAsync(int id);

    // ── Social Icons ──
    Task<IEnumerable<SocialIcon>> GetAllSocialIconsAsync();
    Task<SocialIcon> CreateSocialIconAsync(UpsertSocialIconRequest request);
    Task<SocialIcon?> UpdateSocialIconAsync(int id, UpsertSocialIconRequest request);
    Task<bool> DeleteSocialIconAsync(int id);
}
