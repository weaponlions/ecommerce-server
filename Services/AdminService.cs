using eShopServer.DTOs;
using eShopServer.Interfaces.Repositories;
using eShopServer.Interfaces.Services;
using eShopServer.Models;

namespace eShopServer.Services;

public class AdminService : IAdminService
{
    private readonly IDashboardSectionRepository _sectionRepo;
    private readonly INavbarLinkRepository _navbarRepo;
    private readonly ICarouselSlideRepository _carouselRepo;
    private readonly IProductRepository _productRepo;
    private readonly ICollectionRepository _collectionRepo;
    private readonly IFooterLinkRepository _footerLinkRepo;
    private readonly ISocialIconRepository _socialIconRepo;
    private readonly IMediaAssetRepository _mediaAssetRepo;
    private readonly IMediaUsageRepository _mediaUsageRepo;

    public AdminService(
        IDashboardSectionRepository sectionRepo,
        INavbarLinkRepository navbarRepo,
        ICarouselSlideRepository carouselRepo,
        IProductRepository productRepo,
        ICollectionRepository collectionRepo,
        IFooterLinkRepository footerLinkRepo,
        ISocialIconRepository socialIconRepo,
        IMediaAssetRepository mediaAssetRepo,
        IMediaUsageRepository mediaUsageRepo)
    {
        _sectionRepo = sectionRepo;
        _navbarRepo = navbarRepo;
        _carouselRepo = carouselRepo;
        _productRepo = productRepo;
        _collectionRepo = collectionRepo;
        _footerLinkRepo = footerLinkRepo;
        _socialIconRepo = socialIconRepo;
        _mediaAssetRepo = mediaAssetRepo;
        _mediaUsageRepo = mediaUsageRepo;
    }

    // ════════════════════════════════════════════════════════════════
    //  Dashboard Sections
    // ════════════════════════════════════════════════════════════════

    public async Task<IEnumerable<DashboardSection>> GetAllSectionsAsync()
        => await _sectionRepo.GetAllAsync();

    public async Task<DashboardSection?> UpdateSectionAsync(int id, UpsertDashboardSectionRequest req)
    {
        var section = await _sectionRepo.GetByIdAsync(id);
        if (section is null) return null;

        section.SectionKey   = req.SectionKey;
        section.Title        = req.Title;
        section.DisplayOrder = req.DisplayOrder;
        section.IsVisible    = req.IsVisible;
        section.LayoutHint   = req.LayoutHint;

        return await _sectionRepo.UpdateAsync(section);
    }

    // ════════════════════════════════════════════════════════════════
    //  Navbar Links
    // ════════════════════════════════════════════════════════════════

    public async Task<IEnumerable<NavbarLink>> GetAllNavbarLinksAsync()
        => await _navbarRepo.GetAllAsync();

    public async Task<NavbarLink> CreateNavbarLinkAsync(UpsertNavbarLinkRequest req)
    {
        var link = new NavbarLink(req.Label, req.Url, req.DisplayOrder)
        {
            Icon = req.Icon, IsVisible = req.IsVisible, ParentId = req.ParentId
        };
        return await _navbarRepo.AddAsync(link);
    }

    public async Task<NavbarLink?> UpdateNavbarLinkAsync(int id, UpsertNavbarLinkRequest req)
    {
        var link = await _navbarRepo.GetByIdAsync(id);
        if (link is null) return null;

        link.Label = req.Label; link.Url = req.Url; link.Icon = req.Icon;
        link.DisplayOrder = req.DisplayOrder; link.IsVisible = req.IsVisible; link.ParentId = req.ParentId;

        return await _navbarRepo.UpdateAsync(link);
    }

    public async Task<bool> DeleteNavbarLinkAsync(int id)
        => await _navbarRepo.DeleteAsync(id);

    // ════════════════════════════════════════════════════════════════
    //  Carousel Slides
    // ════════════════════════════════════════════════════════════════

    public async Task<IEnumerable<CarouselSlide>> GetAllCarouselSlidesAsync()
        => await _carouselRepo.GetAllAsync();

    public async Task<CarouselSlide> CreateCarouselSlideAsync(UpsertCarouselSlideRequest req)
    {
        // Resolve ImageUrl from MediaAsset
        var imageUrl = await ResolveImageUrl(req.MediaAssetId);

        var slide = new CarouselSlide(req.Title, imageUrl, req.DisplayOrder)
        {
            Subtitle = req.Subtitle, LinkUrl = req.LinkUrl, ButtonText = req.ButtonText,
            IsVisible = req.IsVisible, StartDate = req.StartDate, EndDate = req.EndDate,
            MediaAssetId = req.MediaAssetId
        };

        var created = await _carouselRepo.AddAsync(slide);
        await TrackMediaUsage(req.MediaAssetId, "CarouselSlide", created.Id, "ImageUrl");
        return created;
    }

    public async Task<CarouselSlide?> UpdateCarouselSlideAsync(int id, UpsertCarouselSlideRequest req)
    {
        var slide = await _carouselRepo.GetByIdAsync(id);
        if (slide is null) return null;

        var imageUrl = await ResolveImageUrl(req.MediaAssetId);

        slide.Title = req.Title; slide.Subtitle = req.Subtitle; slide.ImageUrl = imageUrl;
        slide.LinkUrl = req.LinkUrl; slide.ButtonText = req.ButtonText;
        slide.DisplayOrder = req.DisplayOrder; slide.IsVisible = req.IsVisible;
        slide.StartDate = req.StartDate; slide.EndDate = req.EndDate;

        // Re-link media if asset changed
        if (slide.MediaAssetId != req.MediaAssetId)
        {
            await _mediaUsageRepo.DeleteByEntityAsync("CarouselSlide", id);
            slide.MediaAssetId = req.MediaAssetId;
        }

        var updated = await _carouselRepo.UpdateAsync(slide);
        await TrackMediaUsage(req.MediaAssetId, "CarouselSlide", updated.Id, "ImageUrl");
        return updated;
    }

    public async Task<bool> DeleteCarouselSlideAsync(int id)
    {
        await _mediaUsageRepo.DeleteByEntityAsync("CarouselSlide", id);
        return await _carouselRepo.DeleteAsync(id);
    }

    // ════════════════════════════════════════════════════════════════
    //  Products (legacy dashboard products)
    // ════════════════════════════════════════════════════════════════

    public async Task<IEnumerable<Product>> GetAllProductsAsync()
        => await _productRepo.GetAllAsync();

    public async Task<Product> CreateProductAsync(UpsertProductRequest req)
    {
        var imageUrl = await ResolveImageUrl(req.MediaAssetId);

        var product = new Product(req.Name, req.Price, imageUrl)
        {
            Description = req.Description, OriginalPrice = req.OriginalPrice,
            CategoryLabel = req.CategoryLabel, Badge = req.Badge, Rating = req.Rating,
            ReviewCount = req.ReviewCount, TrendingScore = req.TrendingScore, IsVisible = req.IsVisible,
            MediaAssetId = req.MediaAssetId
        };

        var created = await _productRepo.AddAsync(product);
        await TrackMediaUsage(req.MediaAssetId, "Product", created.Id, "ImageUrl");
        return created;
    }

    public async Task<Product?> UpdateProductAsync(int id, UpsertProductRequest req)
    {
        var product = await _productRepo.GetByIdAsync(id);
        if (product is null) return null;

        var imageUrl = await ResolveImageUrl(req.MediaAssetId);

        product.Name = req.Name; product.Description = req.Description; product.Price = req.Price;
        product.OriginalPrice = req.OriginalPrice; product.ImageUrl = imageUrl;
        product.CategoryLabel = req.CategoryLabel; product.Badge = req.Badge; product.Rating = req.Rating;
        product.ReviewCount = req.ReviewCount; product.TrendingScore = req.TrendingScore; product.IsVisible = req.IsVisible;

        if (product.MediaAssetId != req.MediaAssetId)
        {
            await _mediaUsageRepo.DeleteByEntityAsync("Product", id);
            product.MediaAssetId = req.MediaAssetId;
        }

        var updated = await _productRepo.UpdateAsync(product);
        await TrackMediaUsage(req.MediaAssetId, "Product", updated.Id, "ImageUrl");
        return updated;
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        await _mediaUsageRepo.DeleteByEntityAsync("Product", id);
        return await _productRepo.DeleteAsync(id);
    }

    // ════════════════════════════════════════════════════════════════
    //  Collections
    // ════════════════════════════════════════════════════════════════

    public async Task<IEnumerable<Collection>> GetAllCollectionsAsync()
        => await _collectionRepo.GetAllAsync();

    public async Task<Collection> CreateCollectionAsync(UpsertCollectionRequest req)
    {
        var imageUrl = await ResolveImageUrl(req.MediaAssetId);

        var collection = new Collection(req.Name, imageUrl, req.DisplayOrder)
        {
            Description = req.Description, LinkUrl = req.LinkUrl,
            VisitCount = req.VisitCount, IsVisible = req.IsVisible,
            MediaAssetId = req.MediaAssetId
        };

        var created = await _collectionRepo.AddAsync(collection);
        await TrackMediaUsage(req.MediaAssetId, "Collection", created.Id, "ImageUrl");
        return created;
    }

    public async Task<Collection?> UpdateCollectionAsync(int id, UpsertCollectionRequest req)
    {
        var collection = await _collectionRepo.GetByIdAsync(id);
        if (collection is null) return null;

        var imageUrl = await ResolveImageUrl(req.MediaAssetId);

        collection.Name = req.Name; collection.Description = req.Description; collection.ImageUrl = imageUrl;
        collection.LinkUrl = req.LinkUrl; collection.VisitCount = req.VisitCount;
        collection.DisplayOrder = req.DisplayOrder; collection.IsVisible = req.IsVisible;

        if (collection.MediaAssetId != req.MediaAssetId)
        {
            await _mediaUsageRepo.DeleteByEntityAsync("Collection", id);
            collection.MediaAssetId = req.MediaAssetId;
        }

        var updated = await _collectionRepo.UpdateAsync(collection);
        await TrackMediaUsage(req.MediaAssetId, "Collection", updated.Id, "ImageUrl");
        return updated;
    }

    public async Task<bool> DeleteCollectionAsync(int id)
    {
        await _mediaUsageRepo.DeleteByEntityAsync("Collection", id);
        return await _collectionRepo.DeleteAsync(id);
    }

    // ════════════════════════════════════════════════════════════════
    //  Footer Links
    // ════════════════════════════════════════════════════════════════

    public async Task<IEnumerable<FooterLink>> GetAllFooterLinksAsync()
        => await _footerLinkRepo.GetAllAsync();

    public async Task<FooterLink> CreateFooterLinkAsync(UpsertFooterLinkRequest req)
    {
        var link = new FooterLink(req.GroupName, req.Label, req.Url, req.DisplayOrder)
        {
            IsVisible = req.IsVisible
        };
        return await _footerLinkRepo.AddAsync(link);
    }

    public async Task<FooterLink?> UpdateFooterLinkAsync(int id, UpsertFooterLinkRequest req)
    {
        var link = await _footerLinkRepo.GetByIdAsync(id);
        if (link is null) return null;

        link.GroupName = req.GroupName; link.Label = req.Label; link.Url = req.Url;
        link.DisplayOrder = req.DisplayOrder; link.IsVisible = req.IsVisible;

        return await _footerLinkRepo.UpdateAsync(link);
    }

    public async Task<bool> DeleteFooterLinkAsync(int id)
        => await _footerLinkRepo.DeleteAsync(id);

    // ════════════════════════════════════════════════════════════════
    //  Social Icons
    // ════════════════════════════════════════════════════════════════

    public async Task<IEnumerable<SocialIcon>> GetAllSocialIconsAsync()
        => await _socialIconRepo.GetAllAsync();

    public async Task<SocialIcon> CreateSocialIconAsync(UpsertSocialIconRequest req)
    {
        // IconRef can come from: 1) a CSS class string, or 2) a media asset URL
        var iconRef = req.IconRef ?? "";
        int? mediaAssetId = req.MediaAssetId;

        if (mediaAssetId.HasValue)
        {
            iconRef = await ResolveImageUrl(mediaAssetId.Value);
        }

        if (string.IsNullOrWhiteSpace(iconRef))
            throw new ArgumentException("Either IconRef or MediaAssetId must be provided.");

        var icon = new SocialIcon(req.Platform, iconRef, req.Url, req.DisplayOrder)
        {
            IsVisible = req.IsVisible,
            MediaAssetId = mediaAssetId
        };

        var created = await _socialIconRepo.AddAsync(icon);
        if (mediaAssetId.HasValue)
            await TrackMediaUsage(mediaAssetId.Value, "SocialIcon", created.Id, "IconRef");
        return created;
    }

    public async Task<SocialIcon?> UpdateSocialIconAsync(int id, UpsertSocialIconRequest req)
    {
        var icon = await _socialIconRepo.GetByIdAsync(id);
        if (icon is null) return null;

        var iconRef = req.IconRef ?? "";
        int? mediaAssetId = req.MediaAssetId;

        if (mediaAssetId.HasValue)
        {
            iconRef = await ResolveImageUrl(mediaAssetId.Value);
        }

        if (string.IsNullOrWhiteSpace(iconRef))
            throw new ArgumentException("Either IconRef or MediaAssetId must be provided.");

        icon.Platform = req.Platform; icon.IconRef = iconRef; icon.Url = req.Url;
        icon.DisplayOrder = req.DisplayOrder; icon.IsVisible = req.IsVisible;

        if (icon.MediaAssetId != mediaAssetId)
        {
            await _mediaUsageRepo.DeleteByEntityAsync("SocialIcon", id);
            icon.MediaAssetId = mediaAssetId;
        }

        var updated = await _socialIconRepo.UpdateAsync(icon);
        if (mediaAssetId.HasValue)
            await TrackMediaUsage(mediaAssetId.Value, "SocialIcon", updated.Id, "IconRef");
        return updated;
    }

    public async Task<bool> DeleteSocialIconAsync(int id)
    {
        await _mediaUsageRepo.DeleteByEntityAsync("SocialIcon", id);
        return await _socialIconRepo.DeleteAsync(id);
    }

    // ════════════════════════════════════════════════════════════════
    //  Media Helpers
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Looks up a MediaAsset by ID and returns its URL.
    /// Throws if the asset ID is invalid.
    /// </summary>
    private async Task<string> ResolveImageUrl(int mediaAssetId)
    {
        var asset = await _mediaAssetRepo.GetByIdAsync(mediaAssetId);
        if (asset is null)
            throw new ArgumentException($"Media asset with ID {mediaAssetId} not found. Upload the image first via /api/admin/media/upload.");
        return asset.Url;
    }

    /// <summary>
    /// Creates a MediaUsage record linking the media asset to the entity.
    /// </summary>
    private async Task TrackMediaUsage(int mediaAssetId, string entityType, int entityId, string fieldName)
    {
        var existing = await _mediaUsageRepo.FindExactAsync(
            mediaAssetId, entityType, entityId, fieldName);
        if (existing != null) return;

        var usage = new MediaUsage
        {
            MediaAssetId = mediaAssetId,
            EntityType = entityType,
            EntityId = entityId,
            FieldName = fieldName
        };
        await _mediaUsageRepo.AddAsync(usage);
    }
}
