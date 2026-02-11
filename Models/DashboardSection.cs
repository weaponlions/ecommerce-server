using System.ComponentModel.DataAnnotations;

namespace eShopServer.Models;

/// <summary>
/// Controls the visibility, order, and type of each dashboard section.
/// The frontend renders sections based on this configuration.
/// </summary>
public class DashboardSection
{
    /// <summary>
    /// EF Core requires a parameterless constructor.
    /// </summary>
    private DashboardSection() { }

    public DashboardSection(string sectionKey, string title, int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(sectionKey))
            throw new ArgumentException("SectionKey is required.", nameof(sectionKey));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));
        if (displayOrder < 0)
            throw new ArgumentOutOfRangeException(nameof(displayOrder), "DisplayOrder must be >= 0.");

        SectionKey = sectionKey.Trim();
        Title = title.Trim();
        DisplayOrder = displayOrder;
    }

    public int Id { get; set; }

    /// <summary>
    /// Unique key identifying the section type.
    /// Valid values: "navbar", "carousel", "trending", "recently_visited", "collections", "footer"
    /// </summary>
    [Required(ErrorMessage = "SectionKey is required.")]
    [MaxLength(50, ErrorMessage = "SectionKey cannot exceed 50 characters.")]
    public string SectionKey { get; set; } = string.Empty;

    /// <summary>
    /// Display title for the section (e.g. "Trending Now", "Most Visited Collections").
    /// </summary>
    [Required(ErrorMessage = "Title is required.")]
    [MaxLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Controls the order in which sections appear on the dashboard.
    /// Lower numbers appear first.
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "DisplayOrder must be a non-negative number.")]
    public int DisplayOrder { get; set; }

    /// <summary>
    /// When false, the section is hidden from the dashboard entirely.
    /// </summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// Optional CSS class or layout hint for the frontend.
    /// </summary>
    [MaxLength(50, ErrorMessage = "LayoutHint cannot exceed 50 characters.")]
    public string? LayoutHint { get; set; }
}
