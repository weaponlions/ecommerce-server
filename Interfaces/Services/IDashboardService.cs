using eShopServer.DTOs;

namespace eShopServer.Interfaces.Services;

/// <summary>
/// Service contract for all public-facing dashboard operations.
/// </summary>
public interface IDashboardService
{
    Task<DashboardResponse> GetFullDashboardAsync(string? userId);
    Task<NavbarDto> GetNavbarAsync();
    Task<List<CarouselSlideDto>> GetCarouselAsync();
    Task<List<ProductDto>> GetTrendingAsync();
    Task<List<ProductDto>> GetRecentlyVisitedAsync(string userId);
    Task<bool> TrackVisitAsync(TrackVisitRequest request);
    Task<List<CollectionDto>> GetCollectionsAsync();
    Task<FooterDto> GetFooterAsync();
}
