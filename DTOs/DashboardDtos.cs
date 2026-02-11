namespace eShopServer.DTOs;

// ── Dashboard Full Response ──

public record DashboardResponse(List<DashboardSectionDto> Sections);

public record DashboardSectionDto(
    string SectionKey,
    string Title,
    int DisplayOrder,
    string? LayoutHint,
    object? Data  // Polymorphic: navbar → NavbarDto, carousel → list of slides, etc.
);

// ── Navbar ──

public record NavbarDto(List<NavbarLinkDto> Links);

public record NavbarLinkDto(
    int Id,
    string Label,
    string Url,
    string? Icon,
    int DisplayOrder,
    List<NavbarLinkDto>? Children
);

// ── Carousel ──

public record CarouselSlideDto(
    int Id,
    string Title,
    string? Subtitle,
    string ImageUrl,
    int? MediaAssetId,
    string? LinkUrl,
    string? ButtonText,
    int DisplayOrder
);

// ── Product ──

public record ProductDto(
    int Id,
    string Name,
    string? Description,
    decimal Price,
    decimal? OriginalPrice,
    string ImageUrl,
    int? MediaAssetId,
    string? CategoryLabel,
    string? Badge,
    double Rating,
    int ReviewCount
);

// ── Collection ──

public record CollectionDto(
    int Id,
    string Name,
    string? Description,
    string ImageUrl,
    int? MediaAssetId,
    string? LinkUrl,
    int VisitCount
);

// ── Footer ──

public record FooterDto(
    List<FooterGroupDto> LinkGroups,
    List<SocialIconDto> SocialIcons
);

public record FooterGroupDto(
    string GroupName,
    List<FooterLinkDto> Links
);

public record FooterLinkDto(
    int Id,
    string Label,
    string Url
);

public record SocialIconDto(
    int Id,
    string Platform,
    string IconRef,
    int? MediaAssetId,
    string Url
);

// ── Recently Visited ──

public record TrackVisitRequest(string UserId, int ProductId);

// ── Admin Requests ──

public record UpsertDashboardSectionRequest(
    string SectionKey,
    string Title,
    int DisplayOrder,
    bool IsVisible,
    string? LayoutHint
);

public record UpsertNavbarLinkRequest(
    string Label,
    string Url,
    string? Icon,
    int DisplayOrder,
    bool IsVisible,
    int? ParentId
);

public record UpsertCarouselSlideRequest(
    string Title,
    string? Subtitle,
    int MediaAssetId,          // ← references uploaded media asset
    string? LinkUrl,
    string? ButtonText,
    int DisplayOrder,
    bool IsVisible,
    DateTime? StartDate,
    DateTime? EndDate
);

public record UpsertProductRequest(
    string Name,
    string? Description,
    decimal Price,
    decimal? OriginalPrice,
    int MediaAssetId,          // ← references uploaded media asset
    string? CategoryLabel,
    string? Badge,
    double Rating,
    int ReviewCount,
    int TrendingScore,
    bool IsVisible
);

public record UpsertCollectionRequest(
    string Name,
    string? Description,
    int MediaAssetId,          // ← references uploaded media asset
    string? LinkUrl,
    int VisitCount,
    int DisplayOrder,
    bool IsVisible
);

public record UpsertFooterLinkRequest(
    string GroupName,
    string Label,
    string Url,
    int DisplayOrder,
    bool IsVisible
);

public record UpsertSocialIconRequest(
    string Platform,
    string? IconRef,           // CSS class like "fab fa-facebook" (optional if using media)
    int? MediaAssetId,         // ← uploaded icon image (optional if using CSS class)
    string Url,
    int DisplayOrder,
    bool IsVisible
);
