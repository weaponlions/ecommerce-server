using System.ComponentModel.DataAnnotations;

namespace eShopServer.Models;

public class Collection
{
    /// <summary>
    /// EF Core requires a parameterless constructor.
    /// </summary>
    private Collection() { }

    public Collection(string name, int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));
        if (displayOrder < 0)
            throw new ArgumentOutOfRangeException(nameof(displayOrder), "DisplayOrder must be >= 0.");

        Name = name.Trim();
        DisplayOrder = displayOrder;
    }

    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [MaxLength(200, ErrorMessage = "Name cannot exceed 200 characters.")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
    public string? Description { get; set; }

    [MaxLength(2000, ErrorMessage = "LinkUrl cannot exceed 2000 characters.")]
    public string? LinkUrl { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "VisitCount must be a non-negative number.")]
    public int VisitCount { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "DisplayOrder must be a non-negative number.")]
    public int DisplayOrder { get; set; }

    public bool IsVisible { get; set; } = true;
}
