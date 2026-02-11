using System.ComponentModel.DataAnnotations;

namespace eShopServer.Models;

public class SocialIcon
{
    /// <summary>
    /// EF Core requires a parameterless constructor.
    /// </summary>
    private SocialIcon() { }

    public SocialIcon(string platform, string iconRef, string url, int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(platform))
            throw new ArgumentException("Platform is required.", nameof(platform));
        if (string.IsNullOrWhiteSpace(iconRef))
            throw new ArgumentException("IconRef is required.", nameof(iconRef));
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("Url is required.", nameof(url));
        if (displayOrder < 0)
            throw new ArgumentOutOfRangeException(nameof(displayOrder), "DisplayOrder must be >= 0.");

        Platform = platform.Trim();
        IconRef = iconRef.Trim();
        Url = url.Trim();
        DisplayOrder = displayOrder;
    }

    public int Id { get; set; }

    /// <summary>
    /// Platform name, e.g. "facebook", "instagram", "twitter".
    /// </summary>
    [Required(ErrorMessage = "Platform is required.")]
    [MaxLength(50, ErrorMessage = "Platform cannot exceed 50 characters.")]
    public string Platform { get; set; } = string.Empty;

    /// <summary>
    /// Icon class or URL (e.g. "fab fa-facebook" or an SVG URL).
    /// </summary>
    [Required(ErrorMessage = "IconRef is required.")]
    [MaxLength(200, ErrorMessage = "IconRef cannot exceed 200 characters.")]
    public string IconRef { get; set; } = string.Empty;

    [Required(ErrorMessage = "Url is required.")]
    [MaxLength(500, ErrorMessage = "Url cannot exceed 500 characters.")]
    [Url(ErrorMessage = "Url must be a valid URL.")]
    public string Url { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "DisplayOrder must be a non-negative number.")]
    public int DisplayOrder { get; set; }

    public bool IsVisible { get; set; } = true;
}
