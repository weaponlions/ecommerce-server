using System.ComponentModel.DataAnnotations;

namespace eShopServer.Models;

/// <summary>
/// Represents an uploaded media file (image, icon, etc.) stored in the system.
/// </summary>
public class MediaAsset
{
    /// <summary>
    /// EF Core requires a parameterless constructor.
    /// </summary>
    private MediaAsset() { }

    public MediaAsset(string fileName, string originalFileName, string contentType, long fileSizeBytes, string url)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("FileName is required.", nameof(fileName));
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("Url is required.", nameof(url));

        FileName = fileName.Trim();
        OriginalFileName = originalFileName?.Trim() ?? fileName.Trim();
        ContentType = contentType?.Trim() ?? "application/octet-stream";
        FileSizeBytes = fileSizeBytes;
        Url = url.Trim();
    }

    public int Id { get; set; }

    /// <summary>
    /// System-generated unique filename on disk (e.g. "a3b8d1b6-hero-banner.jpg").
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Original filename as uploaded by the user (e.g. "hero-banner.jpg").
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string OriginalFileName { get; set; } = string.Empty;

    /// <summary>
    /// MIME type of the file (e.g. "image/jpeg", "image/png", "image/svg+xml").
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// File size in bytes.
    /// </summary>
    public long FileSizeBytes { get; set; }

    /// <summary>
    /// Image width in pixels (null for non-image files like SVGs).
    /// </summary>
    public int? Width { get; set; }

    /// <summary>
    /// Image height in pixels (null for non-image files like SVGs).
    /// </summary>
    public int? Height { get; set; }

    /// <summary>
    /// The public URL to access this file (e.g. "/uploads/carousel/a3b8d1b6-hero.jpg").
    /// </summary>
    [Required]
    [MaxLength(2000)]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Alt text for accessibility.
    /// </summary>
    [MaxLength(500)]
    public string? AltText { get; set; }

    /// <summary>
    /// Optional title/caption for the media.
    /// </summary>
    [MaxLength(200)]
    public string? Title { get; set; }

    /// <summary>
    /// Category tag for organizing media: "carousel", "product", "collection",
    /// "category", "social-icon", "general".
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Category { get; set; } = "general";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // ── Navigation ──
    public List<MediaUsage> Usages { get; set; } = [];
}
