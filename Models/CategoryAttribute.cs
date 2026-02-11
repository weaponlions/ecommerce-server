using System.ComponentModel.DataAnnotations;

namespace eShopServer.Models;

/// <summary>
/// Defines a single attribute that a category's products can have.
/// E.g., Category "Shoes" → Attributes: { Size (select), Color (select), Type (select) }
///       Category "Clothes" → Attributes: { Size (select), Color (select), Fabric (string) }
/// </summary>
public class CategoryAttribute
{
    /// <summary>
    /// EF Core requires a parameterless constructor.
    /// </summary>
    private CategoryAttribute() { }

    public CategoryAttribute(int categoryId, string name, string displayName, AttributeDataType dataType)
    {
        if (categoryId <= 0)
            throw new ArgumentOutOfRangeException(nameof(categoryId), "CategoryId must be positive.");
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("DisplayName is required.", nameof(displayName));

        CategoryId = categoryId;
        Name = name.Trim().ToLowerInvariant();
        DisplayName = displayName.Trim();
        DataType = dataType;
    }

    public int Id { get; set; }

    public int CategoryId { get; set; }

    /// <summary>
    /// Machine-readable key: "size", "color", "fabric".
    /// Lowercase, no spaces. Used for filtering and API queries.
    /// </summary>
    [Required(ErrorMessage = "Name is required.")]
    [MaxLength(50, ErrorMessage = "Name cannot exceed 50 characters.")]
    [RegularExpression(@"^[a-z0-9_]+$",
        ErrorMessage = "Name must be lowercase alphanumeric with underscores only.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable label: "Size", "Color", "Fabric Type"
    /// </summary>
    [Required(ErrorMessage = "DisplayName is required.")]
    [MaxLength(100, ErrorMessage = "DisplayName cannot exceed 100 characters.")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Controls what kind of values this attribute accepts.
    /// </summary>
    public AttributeDataType DataType { get; set; }

    /// <summary>
    /// When true, products must have a value for this attribute.
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// When true, this attribute appears as a filter option on the frontend.
    /// </summary>
    public bool IsFilterable { get; set; } = true;

    /// <summary>
    /// JSON array of allowed values for select/multi-select types.
    /// E.g., ["S","M","L","XL"] or ["Red","Blue","Black"]
    /// Null for string/number types.
    /// </summary>
    [MaxLength(4000, ErrorMessage = "Options cannot exceed 4000 characters.")]
    public string? Options { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "DisplayOrder must be non-negative.")]
    public int DisplayOrder { get; set; }

    // ── Navigation ──
    public Category Category { get; set; } = null!;
    public List<ProductAttributeValue> Values { get; set; } = [];
}

/// <summary>
/// Supported data types for category attributes.
/// </summary>
public enum AttributeDataType
{
    /// <summary>Free-form text (e.g. fabric description)</summary>
    String = 0,

    /// <summary>Numeric value (e.g. weight in kg)</summary>
    Number = 1,

    /// <summary>Pick one from Options list (e.g. size: "M")</summary>
    Select = 2,

    /// <summary>Pick multiple from Options list (e.g. colors: ["Red","Blue"])</summary>
    MultiSelect = 3,

    /// <summary>True / False (e.g. waterproof)</summary>
    Boolean = 4,
}
