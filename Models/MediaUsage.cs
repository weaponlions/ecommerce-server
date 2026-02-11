using System.ComponentModel.DataAnnotations;

namespace eShopServer.Models;

/// <summary>
/// Tracks where a MediaAsset is used across the system.
/// E.g. "CarouselSlide #3, field ImageUrl" or "SocialIcon #1, field IconRef".
/// </summary>
public class MediaUsage
{
    public int Id { get; set; }

    /// <summary>
    /// FK to the media asset being used.
    /// </summary>
    public int MediaAssetId { get; set; }

    /// <summary>
    /// The entity type that uses this asset (e.g. "CarouselSlide", "Collection",
    /// "Product", "Category", "SocialIcon").
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// The primary key of the entity that uses this asset.
    /// </summary>
    public int EntityId { get; set; }

    /// <summary>
    /// Which field on the entity holds this reference (e.g. "ImageUrl", "IconRef").
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string FieldName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ── Navigation ──
    public MediaAsset MediaAsset { get; set; } = null!;
}
