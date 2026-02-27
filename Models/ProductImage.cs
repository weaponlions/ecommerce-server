using System.ComponentModel.DataAnnotations;

namespace eShopServer.Models;

/// <summary>
/// Represents an image associated with a product, allowing multiple images per product
/// with specific roles (gallery, hover, listing, etc.).
/// </summary>
public class ProductImage
{
    private ProductImage() { }

    public ProductImage(int productId, int mediaAssetId, string role, int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(role))
            throw new ArgumentException("Role is required.", nameof(role));

        ProductId = productId;
        MediaAssetId = mediaAssetId;
        Role = role.Trim().ToLowerInvariant();
        DisplayOrder = displayOrder;
    }

    public int Id { get; set; }

    public int ProductId { get; set; }

    public int MediaAssetId { get; set; }

    /// <summary>
    /// Role of the image: 'gallery', 'listing', 'hover', 'swatch', 'lifestyle', 'detail'
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Role { get; set; } = "gallery";

    [Range(0, int.MaxValue)]
    public int DisplayOrder { get; set; }

    [MaxLength(500)]
    public string? AltText { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // ── Navigation ──
    [System.Text.Json.Serialization.JsonIgnore]
    public Product Product { get; set; } = null!;
    
    public MediaAsset MediaAsset { get; set; } = null!;
}
