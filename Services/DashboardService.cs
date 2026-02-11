using eShopServer.DTOs;
using eShopServer.Interfaces.Repositories;
using eShopServer.Interfaces.Services;
using eShopServer.Models;

namespace eShopServer.Services;

public class DashboardService : IDashboardService
{
    private const int MaxTrendingProducts = 12;
    private const int MaxRecentlyVisited = 20;
    private const int MaxCollections = 10;

    private readonly IDashboardSectionRepository _sectionRepo;
    private readonly INavbarLinkRepository _navbarRepo;
    private readonly ICarouselSlideRepository _carouselRepo;
    private readonly IProductRepository _productRepo;
    private readonly IRecentlyVisitedProductRepository _recentlyVisitedRepo;
    private readonly ICollectionRepository _collectionRepo;
    private readonly IFooterLinkRepository _footerLinkRepo;
    private readonly ISocialIconRepository _socialIconRepo;

    public DashboardService(
        IDashboardSectionRepository sectionRepo,
        INavbarLinkRepository navbarRepo,
        ICarouselSlideRepository carouselRepo,
        IProductRepository productRepo,
        IRecentlyVisitedProductRepository recentlyVisitedRepo,
        ICollectionRepository collectionRepo,
        IFooterLinkRepository footerLinkRepo,
        ISocialIconRepository socialIconRepo)
    {
        _sectionRepo = sectionRepo;
        _navbarRepo = navbarRepo;
        _carouselRepo = carouselRepo;
        _productRepo = productRepo;
        _recentlyVisitedRepo = recentlyVisitedRepo;
        _collectionRepo = collectionRepo;
        _footerLinkRepo = footerLinkRepo;
        _socialIconRepo = socialIconRepo;
    }

    // ──────────────────────────────────────────────────────────────
    // Full Dashboard
    // ──────────────────────────────────────────────────────────────
    public async Task<DashboardResponse> GetFullDashboardAsync(string? userId)
    {
        var sections = await _sectionRepo.GetVisibleOrderedAsync();
        var result = new List<DashboardSectionDto>();

        foreach (var section in sections)
        {
            object? data = section.SectionKey switch
            {
                "navbar"           => await BuildNavbarDto(),
                "carousel"         => await BuildCarouselDtos(),
                "trending"         => await BuildTrendingDtos(),
                "recently_visited" => userId != null
                                        ? await BuildRecentlyVisitedDtos(userId)
                                        : Array.Empty<ProductDto>(),
                "collections"      => await BuildCollectionDtos(),
                "footer"           => await BuildFooterDto(),
                _                  => null
            };

            result.Add(new DashboardSectionDto(
                section.SectionKey,
                section.Title,
                section.DisplayOrder,
                section.LayoutHint,
                data
            ));
        }

        return new DashboardResponse(result);
    }

    // ──────────────────────────────────────────────────────────────
    // Individual section methods
    // ──────────────────────────────────────────────────────────────

    public async Task<NavbarDto> GetNavbarAsync()
        => await BuildNavbarDto();

    public async Task<List<CarouselSlideDto>> GetCarouselAsync()
        => await BuildCarouselDtos();

    public async Task<List<ProductDto>> GetTrendingAsync()
        => await BuildTrendingDtos();

    public async Task<List<ProductDto>> GetRecentlyVisitedAsync(string userId)
        => await BuildRecentlyVisitedDtos(userId);

    public async Task<bool> TrackVisitAsync(TrackVisitRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
            return false;

        var product = await _productRepo.GetByIdAsync(request.ProductId);
        if (product is null)
            return false;

        // Update existing or add new
        var existing = await _recentlyVisitedRepo.FindByUserAndProductAsync(request.UserId, request.ProductId);
        if (existing != null)
        {
            existing.VisitedAt = DateTime.UtcNow;
            await _recentlyVisitedRepo.UpdateAsync(existing);
        }
        else
        {
            await _recentlyVisitedRepo.AddAsync(
                new RecentlyVisitedProduct(request.UserId, request.ProductId));
        }

        // Enforce per-user limit
        await _recentlyVisitedRepo.RemoveOldestVisitsAsync(request.UserId, MaxRecentlyVisited);
        return true;
    }

    public async Task<List<CollectionDto>> GetCollectionsAsync()
        => await BuildCollectionDtos();

    public async Task<FooterDto> GetFooterAsync()
        => await BuildFooterDto();

    // ──────────────────────────────────────────────────────────────
    // Private builder helpers
    // ──────────────────────────────────────────────────────────────

    private async Task<NavbarDto> BuildNavbarDto()
    {
        var allLinks = (await _navbarRepo.GetVisibleOrderedAsync()).ToList();
        var topLevel = allLinks.Where(l => l.ParentId == null).ToList();

        List<NavbarLinkDto> Map(List<NavbarLink> links) =>
            links.Select(l => new NavbarLinkDto(
                l.Id, l.Label, l.Url, l.Icon, l.DisplayOrder,
                allLinks.Where(c => c.ParentId == l.Id).Any()
                    ? Map(allLinks.Where(c => c.ParentId == l.Id).ToList())
                    : null
            )).ToList();

        return new NavbarDto(Map(topLevel));
    }

    private async Task<List<CarouselSlideDto>> BuildCarouselDtos()
    {
        var slides = await _carouselRepo.GetActiveSlidesAsync(DateTime.UtcNow);
        return slides.Select(s => new CarouselSlideDto(
            s.Id, s.Title, s.Subtitle, s.ImageUrl, s.MediaAssetId,
            s.LinkUrl, s.ButtonText, s.DisplayOrder
        )).ToList();
    }

    private async Task<List<ProductDto>> BuildTrendingDtos()
    {
        var products = await _productRepo.GetTrendingAsync(MaxTrendingProducts);
        return products.Select(p => new ProductDto(
            p.Id, p.Name, p.Description, p.Price, p.OriginalPrice,
            p.ImageUrl, p.MediaAssetId, p.CategoryLabel, p.Badge, p.Rating, p.ReviewCount
        )).ToList();
    }

    private async Task<List<ProductDto>> BuildRecentlyVisitedDtos(string userId)
    {
        var visits = await _recentlyVisitedRepo.GetByUserAsync(userId, MaxRecentlyVisited);
        return visits.Select(r => new ProductDto(
            r.Product.Id, r.Product.Name, r.Product.Description,
            r.Product.Price, r.Product.OriginalPrice, r.Product.ImageUrl, r.Product.MediaAssetId,
            r.Product.CategoryLabel, r.Product.Badge, r.Product.Rating, r.Product.ReviewCount
        )).ToList();
    }

    private async Task<List<CollectionDto>> BuildCollectionDtos()
    {
        var collections = await _collectionRepo.GetMostVisitedAsync(MaxCollections);
        return collections.Select(c => new CollectionDto(
            c.Id, c.Name, c.Description, c.ImageUrl, c.MediaAssetId, c.LinkUrl, c.VisitCount
        )).ToList();
    }

    private async Task<FooterDto> BuildFooterDto()
    {
        var links = (await _footerLinkRepo.GetVisibleOrderedAsync()).ToList();
        var groups = links
            .GroupBy(l => l.GroupName)
            .Select(g => new FooterGroupDto(
                g.Key,
                g.Select(l => new FooterLinkDto(l.Id, l.Label, l.Url)).ToList()
            )).ToList();

        var socials = (await _socialIconRepo.GetVisibleOrderedAsync())
            .Select(s => new SocialIconDto(s.Id, s.Platform, s.IconRef, s.MediaAssetId, s.Url))
            .ToList();

        return new FooterDto(groups, socials);
    }
}
