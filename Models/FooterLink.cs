using System.ComponentModel.DataAnnotations;

namespace eShopServer.Models;

public class FooterLink
{
    /// <summary>
    /// EF Core requires a parameterless constructor.
    /// </summary>
    private FooterLink() { }

    public FooterLink(string groupName, string label, string url, int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(groupName))
            throw new ArgumentException("GroupName is required.", nameof(groupName));
        if (string.IsNullOrWhiteSpace(label))
            throw new ArgumentException("Label is required.", nameof(label));
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("Url is required.", nameof(url));
        if (displayOrder < 0)
            throw new ArgumentOutOfRangeException(nameof(displayOrder), "DisplayOrder must be >= 0.");

        GroupName = groupName.Trim();
        Label = label.Trim();
        Url = url.Trim();
        DisplayOrder = displayOrder;
    }

    public int Id { get; set; }

    /// <summary>
    /// Group name, e.g. "Company", "Help", "Legal". 
    /// Links are grouped by this in the frontend.
    /// </summary>
    [Required(ErrorMessage = "GroupName is required.")]
    [MaxLength(100, ErrorMessage = "GroupName cannot exceed 100 characters.")]
    public string GroupName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Label is required.")]
    [MaxLength(100, ErrorMessage = "Label cannot exceed 100 characters.")]
    public string Label { get; set; } = string.Empty;

    [Required(ErrorMessage = "Url is required.")]
    [MaxLength(500, ErrorMessage = "Url cannot exceed 500 characters.")]
    public string Url { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "DisplayOrder must be a non-negative number.")]
    public int DisplayOrder { get; set; }

    public bool IsVisible { get; set; } = true;
}
