using System.ComponentModel.DataAnnotations;

namespace eShopServer.Models;

/// <summary>
/// Defines a product category (e.g. "Shoes", "Clothes", "Electronics").
/// Each category determines which attributes its products can have.
/// </summary>
public class Category
{
    /// <summary>
    /// EF Core requires a parameterless constructor.
    /// </summary>
    private Category() { }

    public Category(string name, string slug)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Slug is required.", nameof(slug));

        Name = name.Trim();
        Slug = slug.Trim().ToLowerInvariant();
    }

    public int Id { get; set; }

    /// <summary>
    /// Display name: "Shoes", "Clothes", "Electronics"
    /// </summary>
    [Required(ErrorMessage = "Name is required.")]
    [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// URL-friendly identifier: "shoes", "clothes", "electronics".
    /// Must be unique.
    /// </summary>
    [Required(ErrorMessage = "Slug is required.")]
    [MaxLength(100, ErrorMessage = "Slug cannot exceed 100 characters.")]
    [RegularExpression(@"^[a-z0-9]+(?:-[a-z0-9]+)*$",
        ErrorMessage = "Slug must be lowercase alphanumeric with hyphens only.")]
    public string Slug { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
    public string? Description { get; set; }

    /// <summary>
    /// FK to MediaAsset — the category's image.
    /// </summary>
    public int? MediaAssetId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ── Navigation ──
    public MediaAsset? MediaAsset { get; set; }
    public List<CategoryAttribute> Attributes { get; set; } = [];
    public List<Product> Products { get; set; } = [];
}
