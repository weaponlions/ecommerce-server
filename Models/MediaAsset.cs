using System.ComponentModel.DataAnnotations;

namespace eShopServer.Models;

/// <summary>
/// Represents an uploaded media file stored on the server.
/// Contains metadata and the public URL to access the file.
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

        FileName = fileName;
        OriginalFileName = originalFileName;
        ContentType = contentType;
        FileSizeBytes = fileSizeBytes;
        Url = url;
    }

    public int Id { get; set; }

    /// <summary>
    /// Unique filename on disk (GUID-based).
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Original filename as uploaded by the user.
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string OriginalFileName { get; set; } = string.Empty;

    /// <summary>
    /// MIME type, e.g. "image/jpeg".
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// File size in bytes.
    /// </summary>
    public long FileSizeBytes { get; set; }

    /// <summary>
    /// Image width in pixels (null for non-raster or unknown).
    /// </summary>
    public int? Width { get; set; }

    /// <summary>
    /// Image height in pixels (null for non-raster or unknown).
    /// </summary>
    public int? Height { get; set; }

    /// <summary>
    /// Public URL to access the stored file.
    /// </summary>
    [Required]
    [MaxLength(2000)]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Accessibility alt text.
    /// </summary>
    [MaxLength(500)]
    public string? AltText { get; set; }

    /// <summary>
    /// Optional display title.
    /// </summary>
    [MaxLength(200)]
    public string? Title { get; set; }

    /// <summary>
    /// Organizational category: "carousel", "product", "collection", "category", "social-icon", "general".
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Category { get; set; } = "general";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // ── Navigation ──
    public List<MediaUsage> Usages { get; set; } = [];
}
