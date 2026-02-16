using System.ComponentModel.DataAnnotations;

namespace eShopServer.Models;

public class Product
{
    /// <summary>
    /// EF Core requires a parameterless constructor.
    /// </summary>
    private Product() { }

    public Product(string name, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));
        if (price < 0)
            throw new ArgumentOutOfRangeException(nameof(price), "Price must be >= 0.");

        Name = name.Trim();
        Price = price;
    }

    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [MaxLength(200, ErrorMessage = "Name cannot exceed 200 characters.")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
    public string? Description { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Price must be a non-negative number.")]
    public decimal Price { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "OriginalPrice must be a non-negative number.")]
    public decimal? OriginalPrice { get; set; }

    [MaxLength(100, ErrorMessage = "CategoryLabel cannot exceed 100 characters.")]
    public string? CategoryLabel { get; set; }

    [MaxLength(50, ErrorMessage = "Badge cannot exceed 50 characters.")]
    public string? Badge { get; set; }

    [Range(0, 5, ErrorMessage = "Rating must be between 0 and 5.")]
    public double Rating { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "ReviewCount must be a non-negative number.")]
    public int ReviewCount { get; set; }

    /// <summary>
    /// Used to determine trending products — higher = more trending.
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "TrendingScore must be a non-negative number.")]
    public int TrendingScore { get; set; }

    /// <summary>
    /// FK to Category. Nullable for backward compatibility with existing products.
    /// </summary>
    public int? CategoryId { get; set; }

    /// <summary>
    /// Available stock quantity. -1 means unlimited/untracked.
    /// </summary>
    [Range(-1, int.MaxValue, ErrorMessage = "Stock must be -1 (unlimited) or >= 0.")]
    public int Stock { get; set; } = -1;

    /// <summary>
    /// FK to MediaAsset — the product's image.
    /// </summary>
    public int? MediaAssetId { get; set; }

    public bool IsVisible { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ── Navigation ──
    public Category? Category { get; set; }
    public MediaAsset? MediaAsset { get; set; }
    public List<ProductAttributeValue> AttributeValues { get; set; } = [];
}
