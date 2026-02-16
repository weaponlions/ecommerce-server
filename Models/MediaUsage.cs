using System.ComponentModel.DataAnnotations;

namespace eShopServer.Models;

/// <summary>
/// Tracks where a MediaAsset is used (e.g., which entity and field).
/// </summary>
public class MediaUsage
{
    public int Id { get; set; }

    /// <summary>
    /// FK to the MediaAsset.
    /// </summary>
    public int MediaAssetId { get; set; }

    /// <summary>
    /// Entity type name, e.g. "CarouselSlide", "Product", "Collection".
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// The Id of the entity that uses this asset.
    /// </summary>
    public int EntityId { get; set; }

    /// <summary>
    /// Which field on the entity uses this asset, e.g. "ImageUrl", "IconRef".
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string FieldName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ── Navigation ──
    public MediaAsset MediaAsset { get; set; } = null!;
}
