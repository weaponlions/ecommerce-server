using System.ComponentModel.DataAnnotations;

namespace eShopServer.Models;

/// <summary>
/// Stores a single attribute value for a product.
/// E.g., Product "Nike Air Max" → { Attribute: "size", Value: "42" }
///                                 { Attribute: "color", Value: "White" }
/// </summary>
public class ProductAttributeValue
{
    /// <summary>
    /// EF Core requires a parameterless constructor.
    /// </summary>
    private ProductAttributeValue() { }

    public ProductAttributeValue(int productId, int categoryAttributeId, string value)
    {
        if (productId <= 0)
            throw new ArgumentOutOfRangeException(nameof(productId), "ProductId must be positive.");
        if (categoryAttributeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(categoryAttributeId), "CategoryAttributeId must be positive.");
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value is required.", nameof(value));

        ProductId = productId;
        CategoryAttributeId = categoryAttributeId;
        Value = value.Trim();
    }

    public int Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "ProductId must be positive.")]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "CategoryAttributeId must be positive.")]
    public int CategoryAttributeId { get; set; }

    /// <summary>
    /// The attribute value stored as a string.
    /// - String: raw text
    /// - Number: numeric string (e.g. "42")
    /// - Select: one of the allowed options
    /// - MultiSelect: JSON array (e.g. ["Red","Blue"])
    /// - Boolean: "true" or "false"
    /// </summary>
    [Required(ErrorMessage = "Value is required.")]
    [MaxLength(2000, ErrorMessage = "Value cannot exceed 2000 characters.")]
    public string Value { get; set; } = string.Empty;

    // ── Navigation ──
    public Product Product { get; set; } = null!;
    public CategoryAttribute CategoryAttribute { get; set; } = null!;
}
