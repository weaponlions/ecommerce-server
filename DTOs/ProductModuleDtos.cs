namespace eShopServer.DTOs;

// ══════════════════════════════════════════════════════════════
//  Category DTOs
// ══════════════════════════════════════════════════════════════

public record CategoryResponse(
    int Id,
    string Name,
    string Slug,
    string? Description,
    string? ImageUrl,
    int? MediaAssetId,
    bool IsActive,
    List<CategoryAttributeResponse>? Attributes
);

public record CategoryListResponse(
    int Id,
    string Name,
    string Slug,
    string? Description,
    string? ImageUrl,
    int? MediaAssetId
);

public record UpsertCategoryRequest(
    string Name,
    string Slug,
    string? Description,
    int? MediaAssetId,         // ← references uploaded media asset
    bool IsActive
);

// ══════════════════════════════════════════════════════════════
//  Category Attribute DTOs
// ══════════════════════════════════════════════════════════════

public record CategoryAttributeResponse(
    int Id,
    string Name,
    string DisplayName,
    string DataType,
    bool IsRequired,
    bool IsFilterable,
    string[]? Options,
    int DisplayOrder
);

public record UpsertCategoryAttributeRequest(
    string Name,
    string DisplayName,
    string DataType,
    bool IsRequired,
    bool IsFilterable,
    string[]? Options,
    int DisplayOrder
);

// ══════════════════════════════════════════════════════════════
//  Product Module DTOs (extended)
// ══════════════════════════════════════════════════════════════

public record ProductDetailResponse(
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
    int ReviewCount,
    int Stock,
    int? CategoryId,
    string? CategoryName,
    string? CategorySlug,
    Dictionary<string, object?> Attributes
);

public record ProductListItemResponse(
    int Id,
    string Name,
    decimal Price,
    decimal? OriginalPrice,
    string ImageUrl,
    int? MediaAssetId,
    string? CategoryLabel,
    string? Badge,
    double Rating,
    int ReviewCount,
    int Stock
);

public record CreateProductRequest(
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
    bool IsVisible,
    int? CategoryId,
    int Stock,
    Dictionary<string, string>? Attributes
);

public record UpdateProductRequest(
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
    bool IsVisible,
    int? CategoryId,
    int Stock,
    Dictionary<string, string>? Attributes
);

// ══════════════════════════════════════════════════════════════
//  Product Filtering
// ══════════════════════════════════════════════════════════════

public record ProductFilterRequest(
    int? CategoryId,
    string? CategorySlug,
    decimal? MinPrice,
    decimal? MaxPrice,
    string? Search,
    Dictionary<string, string>? Attributes,
    string? SortBy,
    bool SortDescending,
    int Page,
    int PageSize
);

public record PagedResponse<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);
