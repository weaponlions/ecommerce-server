namespace eShopServer.DTOs;

// ══════════════════════════════════════════════════════════════
//  Media Asset Responses
// ══════════════════════════════════════════════════════════════

public record MediaAssetResponse(
    int Id,
    string FileName,
    string OriginalFileName,
    string ContentType,
    long FileSizeBytes,
    string FileSize,          // Human-readable: "1.2 MB"
    int? Width,
    int? Height,
    string Url,
    string? AltText,
    string? Title,
    string Category,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    int UsageCount
);

public record MediaAssetDetailResponse(
    int Id,
    string FileName,
    string OriginalFileName,
    string ContentType,
    long FileSizeBytes,
    string FileSize,
    int? Width,
    int? Height,
    string Url,
    string? AltText,
    string? Title,
    string Category,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<MediaUsageResponse> Usages
);

// ══════════════════════════════════════════════════════════════
//  Media Usage Responses
// ══════════════════════════════════════════════════════════════

public record MediaUsageResponse(
    int Id,
    string EntityType,
    int EntityId,
    string FieldName,
    DateTime CreatedAt
);

// ══════════════════════════════════════════════════════════════
//  Requests
// ══════════════════════════════════════════════════════════════

/// <summary>
/// Sent as multipart/form-data along with the file upload.
/// </summary>
public record UploadMediaRequest(
    string? AltText,
    string? Title,
    string Category    // "carousel", "product", "collection", "category", "social-icon", "general"
);

public record UpdateMediaMetadataRequest(
    string? AltText,
    string? Title,
    string? Category
);

public record LinkMediaRequest(
    int MediaAssetId,
    string EntityType,   // "CarouselSlide", "Product", "Collection", "Category", "SocialIcon"
    int EntityId,
    string FieldName     // "ImageUrl", "IconRef", etc.
);
