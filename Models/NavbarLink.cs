using System.ComponentModel.DataAnnotations;

namespace eShopServer.Models;

public class NavbarLink
{
    /// <summary>
    /// EF Core requires a parameterless constructor.
    /// </summary>
    private NavbarLink() { }

    public NavbarLink(string label, string url, int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(label))
            throw new ArgumentException("Label is required.", nameof(label));
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("Url is required.", nameof(url));
        if (displayOrder < 0)
            throw new ArgumentOutOfRangeException(nameof(displayOrder), "DisplayOrder must be >= 0.");

        Label = label.Trim();
        Url = url.Trim();
        DisplayOrder = displayOrder;
    }

    public int Id { get; set; }

    [Required(ErrorMessage = "Label is required.")]
    [MaxLength(100, ErrorMessage = "Label cannot exceed 100 characters.")]
    public string Label { get; set; } = string.Empty;

    [Required(ErrorMessage = "Url is required.")]
    [MaxLength(500, ErrorMessage = "Url cannot exceed 500 characters.")]
    public string Url { get; set; } = string.Empty;

    [MaxLength(100, ErrorMessage = "Icon cannot exceed 100 characters.")]
    public string? Icon { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "DisplayOrder must be a non-negative number.")]
    public int DisplayOrder { get; set; }

    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// Null for top-level links; set to parent's Id for dropdown children.
    /// </summary>
    public int? ParentId { get; set; }
}
