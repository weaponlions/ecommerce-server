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
        // Validate the media asset exists (only if provided)
        if (req.MediaAssetId.HasValue)
            await ValidateMediaAsset(req.MediaAssetId.Value);

        var slide = new CarouselSlide(req.Title, req.DisplayOrder)
        {
            Subtitle = req.Subtitle, MediaAssetId = req.MediaAssetId,
            LinkUrl = req.LinkUrl, ButtonText = req.ButtonText,
            IsVisible = req.IsVisible, StartDate = req.StartDate, EndDate = req.EndDate
        };

        var created = await _carouselRepo.AddAsync(slide);

        if (req.MediaAssetId.HasValue)
            await TrackMediaUsage(req.MediaAssetId.Value, "CarouselSlide", created.Id, "MediaAssetId");

        return created;
    }

    public async Task<CarouselSlide?> UpdateCarouselSlideAsync(int id, UpsertCarouselSlideRequest req)
    {
        var slide = await _carouselRepo.GetByIdAsync(id);
        if (slide is null) return null;

        // Validate the media asset exists (only if provided)
        if (req.MediaAssetId.HasValue)
            await ValidateMediaAsset(req.MediaAssetId.Value);

        slide.Title = req.Title; slide.Subtitle = req.Subtitle;
        slide.MediaAssetId = req.MediaAssetId;
        slide.LinkUrl = req.LinkUrl; slide.ButtonText = req.ButtonText;
        slide.DisplayOrder = req.DisplayOrder; slide.IsVisible = req.IsVisible;
        slide.StartDate = req.StartDate; slide.EndDate = req.EndDate;

        var updated = await _carouselRepo.UpdateAsync(slide);

        if (req.MediaAssetId.HasValue)
            await TrackMediaUsage(req.MediaAssetId.Value, "CarouselSlide", id, "MediaAssetId");

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
        // Validate the media asset exists
        await ValidateMediaAsset(req.MediaAssetId);

        var product = new Product(req.Name, req.Price)
        {
            Description = req.Description, OriginalPrice = req.OriginalPrice,
            MediaAssetId = req.MediaAssetId,
            CategoryLabel = req.CategoryLabel, Badge = req.Badge, Rating = req.Rating,
            ReviewCount = req.ReviewCount, TrendingScore = req.TrendingScore, IsVisible = req.IsVisible
        };

        var created = await _productRepo.AddAsync(product);
        await TrackMediaUsage(req.MediaAssetId, "Product", created.Id, "MediaAssetId");
        return created;
    }

    public async Task<Product?> UpdateProductAsync(int id, UpsertProductRequest req)
    {
        var product = await _productRepo.GetByIdAsync(id);
        if (product is null) return null;

        // Validate the media asset exists
        await ValidateMediaAsset(req.MediaAssetId);

        product.Name = req.Name; product.Description = req.Description; product.Price = req.Price;
        product.OriginalPrice = req.OriginalPrice; product.MediaAssetId = req.MediaAssetId;
        product.CategoryLabel = req.CategoryLabel; product.Badge = req.Badge; product.Rating = req.Rating;
        product.ReviewCount = req.ReviewCount; product.TrendingScore = req.TrendingScore; product.IsVisible = req.IsVisible;

        var updated = await _productRepo.UpdateAsync(product);
        await TrackMediaUsage(req.MediaAssetId, "Product", id, "MediaAssetId");
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
        // Validate the media asset exists

        if (req.MediaAssetId.HasValue)
            await ValidateMediaAsset(req.MediaAssetId.Value);

        var collection = new Collection(req.Name, req.DisplayOrder)
        {
            Description = req.Description, MediaAssetId = req.MediaAssetId,
            LinkUrl = req.LinkUrl, VisitCount = req.VisitCount, IsVisible = req.IsVisible
        };

        var created = await _collectionRepo.AddAsync(collection);
        if (req.MediaAssetId.HasValue)
            await TrackMediaUsage(req.MediaAssetId.Value, "Collection", created.Id, "MediaAssetId");
        return created;
    }

    public async Task<Collection?> UpdateCollectionAsync(int id, UpsertCollectionRequest req)
    {
        var collection = await _collectionRepo.GetByIdAsync(id);
        if (collection is null) return null;

        // Validate the media asset exists
        if (req.MediaAssetId.HasValue)
            await ValidateMediaAsset(req.MediaAssetId.Value);

        collection.Name = req.Name; collection.Description = req.Description;
        collection.MediaAssetId = req.MediaAssetId;
        collection.LinkUrl = req.LinkUrl; collection.VisitCount = req.VisitCount;
        collection.DisplayOrder = req.DisplayOrder; collection.IsVisible = req.IsVisible;

        var updated = await _collectionRepo.UpdateAsync(collection);
        if (req.MediaAssetId.HasValue)
            await TrackMediaUsage(req.MediaAssetId.Value, "Collection", id, "MediaAssetId");
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
        var iconRef = req.IconRef ?? "";

        // If a media asset is provided, validate it and use its URL as the icon ref
        if (req.MediaAssetId.HasValue)
        {
            var asset = await _mediaAssetRepo.GetByIdAsync(req.MediaAssetId.Value);
            if (asset is null)
                throw new ArgumentException($"Media asset with ID {req.MediaAssetId} not found.");
            iconRef = asset.Url;
        }

        var icon = new SocialIcon(req.Platform, iconRef, req.Url, req.DisplayOrder)
        {
            MediaAssetId = req.MediaAssetId,
            IsVisible = req.IsVisible
        };

        var created = await _socialIconRepo.AddAsync(icon);

        if (req.MediaAssetId.HasValue)
            await TrackMediaUsage(req.MediaAssetId.Value, "SocialIcon", created.Id, "MediaAssetId");

        return created;
    }

    public async Task<SocialIcon?> UpdateSocialIconAsync(int id, UpsertSocialIconRequest req)
    {
        var icon = await _socialIconRepo.GetByIdAsync(id);
        if (icon is null) return null;

        icon.Platform = req.Platform; icon.Url = req.Url;
        icon.DisplayOrder = req.DisplayOrder; icon.IsVisible = req.IsVisible;

        var iconRef = req.IconRef ?? "";
        if (req.MediaAssetId.HasValue)
        {
            var asset = await _mediaAssetRepo.GetByIdAsync(req.MediaAssetId.Value);
            if (asset is null)
                throw new ArgumentException($"Media asset with ID {req.MediaAssetId} not found.");
            iconRef = asset.Url;
        }

        icon.IconRef = iconRef;
        icon.MediaAssetId = req.MediaAssetId;

        var updated = await _socialIconRepo.UpdateAsync(icon);

        if (req.MediaAssetId.HasValue)
            await TrackMediaUsage(req.MediaAssetId.Value, "SocialIcon", id, "MediaAssetId");

        return updated;
    }

    public async Task<bool> DeleteSocialIconAsync(int id)
    {
        await _mediaUsageRepo.DeleteByEntityAsync("SocialIcon", id);
        return await _socialIconRepo.DeleteAsync(id);
    }

    // ════════════════════════════════════════════════════════════════
    //  Private Helpers
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Validates that a media asset with the given ID exists.
    /// </summary>
    private async Task ValidateMediaAsset(int mediaAssetId)
    {
        var asset = await _mediaAssetRepo.GetByIdAsync(mediaAssetId);
        if (asset is null)
            throw new ArgumentException(
                $"Media asset with ID {mediaAssetId} not found. Upload the image first via /api/admin/media/upload.");
    }

    /// <summary>
    /// Creates a MediaUsage record to track that an entity uses a specific media asset.
    /// Avoids creating duplicates.
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
