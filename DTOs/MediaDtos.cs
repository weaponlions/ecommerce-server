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
    string FileSizeFormatted,
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
    string FileSizeFormatted,
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

public record MediaUsageResponse(
    int Id,
    string EntityType,
    int EntityId,
    string FieldName,
    DateTime CreatedAt
);

// ══════════════════════════════════════════════════════════════
//  Media Requests
// ══════════════════════════════════════════════════════════════

public record UpdateMediaMetadataRequest(
    string? AltText,
    string? Title,
    string? Category
);

public record LinkMediaRequest(
    int MediaAssetId,
    string EntityType,
    int EntityId,
    string FieldName
);
