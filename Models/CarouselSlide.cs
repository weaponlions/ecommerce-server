using System.ComponentModel.DataAnnotations;

namespace eShopServer.Models;

public class CarouselSlide
{
    /// <summary>
    /// EF Core requires a parameterless constructor.
    /// </summary>
    private CarouselSlide() { }

    public CarouselSlide(string title, int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));
        if (displayOrder < 0)
            throw new ArgumentOutOfRangeException(nameof(displayOrder), "DisplayOrder must be >= 0.");

        Title = title.Trim();
        DisplayOrder = displayOrder;
    }

    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "Subtitle cannot exceed 500 characters.")]
    public string? Subtitle { get; set; }

    [MaxLength(2000, ErrorMessage = "LinkUrl cannot exceed 2000 characters.")]
    public string? LinkUrl { get; set; }

    [MaxLength(100, ErrorMessage = "ButtonText cannot exceed 100 characters.")]
    public string? ButtonText { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "DisplayOrder must be a non-negative number.")]
    public int DisplayOrder { get; set; }

    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// Optional scheduling: slide only shows between these dates.
    /// EndDate must be after StartDate when both are provided.
    /// </summary>
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartDate.HasValue && EndDate.HasValue && EndDate < StartDate)
        {
            yield return new ValidationResult(
                "EndDate must be after StartDate.",
                new[] { nameof(EndDate) });
        }
    }
}
