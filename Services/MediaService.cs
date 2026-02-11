using eShopServer.DTOs;
using eShopServer.Interfaces.Repositories;
using eShopServer.Interfaces.Services;
using eShopServer.Models;

namespace eShopServer.Services;

public class MediaService : IMediaService
{
    private readonly IMediaAssetRepository _assetRepo;
    private readonly IMediaUsageRepository _usageRepo;
    private readonly IWebHostEnvironment _env;
    private readonly string _uploadRoot;

    // Allowed image MIME types
    private static readonly HashSet<string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/gif", "image/webp",
        "image/svg+xml", "image/bmp", "image/x-icon", "image/vnd.microsoft.icon"
    };

    // Allowed categories
    private static readonly HashSet<string> AllowedCategories = new(StringComparer.OrdinalIgnoreCase)
    {
        "carousel", "product", "collection", "category", "social-icon", "general"
    };

    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    public MediaService(
        IMediaAssetRepository assetRepo,
        IMediaUsageRepository usageRepo,
        IWebHostEnvironment env)
    {
        _assetRepo = assetRepo;
        _usageRepo = usageRepo;
        _env = env;

        // Store uploads in wwwroot/uploads
        _uploadRoot = Path.Combine(_env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"), "uploads");
    }

    // ════════════════════════════════════════════════════════════════
    //  Upload
    // ════════════════════════════════════════════════════════════════

    public async Task<MediaAssetResponse> UploadAsync(
        IFormFile file, string? altText, string? title, string category)
    {
        // ── Validate ──
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is empty or missing.");

        if (file.Length > MaxFileSizeBytes)
            throw new ArgumentException($"File exceeds maximum size of {MaxFileSizeBytes / (1024 * 1024)} MB.");

        if (!AllowedTypes.Contains(file.ContentType))
            throw new ArgumentException($"File type '{file.ContentType}' is not allowed. Allowed: {string.Join(", ", AllowedTypes)}");

        var normalizedCategory = category?.Trim().ToLowerInvariant() ?? "general";
        if (!AllowedCategories.Contains(normalizedCategory))
            throw new ArgumentException($"Invalid category '{category}'. Allowed: {string.Join(", ", AllowedCategories)}");

        // ── Generate unique filename ──
        var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant() ?? "";
        var originalName = Path.GetFileNameWithoutExtension(file.FileName);
        var safeName = SanitizeFileName(originalName);
        var uniqueFileName = $"{Guid.NewGuid():N}-{safeName}{extension}";

        // ── Create category subfolder ──
        var categoryFolder = Path.Combine(_uploadRoot, normalizedCategory);
        Directory.CreateDirectory(categoryFolder);

        var filePath = Path.Combine(categoryFolder, uniqueFileName);

        // ── Save file to disk ──
        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // ── Get image dimensions if applicable ──
        int? width = null;
        int? height = null;
        // Skip dimension detection for SVGs and non-raster images
        if (file.ContentType != "image/svg+xml")
        {
            try
            {
                (width, height) = GetImageDimensions(filePath);
            }
            catch
            {
                // Non-critical; leave as null
            }
        }

        // ── Build public URL ──
        var url = $"/uploads/{normalizedCategory}/{uniqueFileName}";

        // ── Save to database ──
        var asset = new MediaAsset(uniqueFileName, file.FileName, file.ContentType, file.Length, url)
        {
            AltText = altText?.Trim(),
            Title = title?.Trim(),
            Category = normalizedCategory,
            Width = width,
            Height = height
        };

        await _assetRepo.AddAsync(asset);

        return MapToResponse(asset);
    }

    // ════════════════════════════════════════════════════════════════
    //  CRUD
    // ════════════════════════════════════════════════════════════════

    public async Task<MediaAssetDetailResponse?> GetByIdAsync(int id)
    {
        var asset = await _assetRepo.GetWithUsagesAsync(id);
        return asset is null ? null : MapToDetailResponse(asset);
    }

    public async Task<PagedResponse<MediaAssetResponse>> GetAllAsync(
        string? search, string? category, int page, int pageSize)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var totalCount = await _assetRepo.GetTotalCountAsync(search, category);
        var items = await _assetRepo.SearchAsync(search, category, page, pageSize);

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedResponse<MediaAssetResponse>(
            items.Select(MapToResponse).ToList(),
            totalCount,
            page,
            pageSize,
            totalPages
        );
    }

    public async Task<MediaAssetResponse?> UpdateMetadataAsync(int id, UpdateMediaMetadataRequest request)
    {
        var asset = await _assetRepo.GetByIdAsync(id);
        if (asset is null) return null;

        if (request.AltText is not null)
            asset.AltText = request.AltText.Trim();
        if (request.Title is not null)
            asset.Title = request.Title.Trim();
        if (request.Category is not null)
        {
            var cat = request.Category.Trim().ToLowerInvariant();
            if (!AllowedCategories.Contains(cat))
                throw new ArgumentException($"Invalid category '{request.Category}'.");

            // Move file to new category folder if category changed
            if (asset.Category != cat)
            {
                var oldPath = Path.Combine(_uploadRoot, asset.Category, asset.FileName);
                var newFolder = Path.Combine(_uploadRoot, cat);
                Directory.CreateDirectory(newFolder);
                var newPath = Path.Combine(newFolder, asset.FileName);

                if (File.Exists(oldPath))
                    File.Move(oldPath, newPath);

                asset.Url = $"/uploads/{cat}/{asset.FileName}";
                asset.Category = cat;
            }
        }

        asset.UpdatedAt = DateTime.UtcNow;
        await _assetRepo.UpdateAsync(asset);

        return MapToResponse(asset);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var asset = await _assetRepo.GetWithUsagesAsync(id);
        if (asset is null) return false;

        // Delete file from disk
        var filePath = Path.Combine(_uploadRoot, asset.Category, asset.FileName);
        if (File.Exists(filePath))
            File.Delete(filePath);

        // Delete all usages first
        await _usageRepo.DeleteByMediaAssetIdAsync(id);

        // Delete the asset record
        return await _assetRepo.DeleteAsync(id);
    }

    // ════════════════════════════════════════════════════════════════
    //  Usage Tracking
    // ════════════════════════════════════════════════════════════════

    public async Task<MediaUsageResponse?> LinkAsync(LinkMediaRequest request)
    {
        // Verify the asset exists
        var asset = await _assetRepo.GetByIdAsync(request.MediaAssetId);
        if (asset is null) return null;

        // Check for duplicate link
        var existing = await _usageRepo.FindExactAsync(
            request.MediaAssetId, request.EntityType, request.EntityId, request.FieldName);
        if (existing is not null)
            return MapUsageResponse(existing); // Already linked, return existing

        var usage = new MediaUsage
        {
            MediaAssetId = request.MediaAssetId,
            EntityType = request.EntityType.Trim(),
            EntityId = request.EntityId,
            FieldName = request.FieldName.Trim()
        };

        await _usageRepo.AddAsync(usage);
        return MapUsageResponse(usage);
    }

    public async Task<bool> UnlinkAsync(int usageId)
        => await _usageRepo.DeleteAsync(usageId);

    public async Task<IEnumerable<MediaUsageResponse>> GetUsagesForAssetAsync(int mediaAssetId)
    {
        var usages = await _usageRepo.GetByMediaAssetIdAsync(mediaAssetId);
        return usages.Select(MapUsageResponse);
    }

    public async Task<IEnumerable<MediaAssetResponse>> GetMediaForEntityAsync(
        string entityType, int entityId)
    {
        var usages = await _usageRepo.GetByEntityAsync(entityType, entityId);
        return usages.Select(u => MapToResponse(u.MediaAsset));
    }

    // ════════════════════════════════════════════════════════════════
    //  Helpers
    // ════════════════════════════════════════════════════════════════

    private static MediaAssetResponse MapToResponse(MediaAsset asset) => new(
        asset.Id,
        asset.FileName,
        asset.OriginalFileName,
        asset.ContentType,
        asset.FileSizeBytes,
        FormatFileSize(asset.FileSizeBytes),
        asset.Width,
        asset.Height,
        asset.Url,
        asset.AltText,
        asset.Title,
        asset.Category,
        asset.CreatedAt,
        asset.UpdatedAt,
        asset.Usages?.Count ?? 0
    );

    private static MediaAssetDetailResponse MapToDetailResponse(MediaAsset asset) => new(
        asset.Id,
        asset.FileName,
        asset.OriginalFileName,
        asset.ContentType,
        asset.FileSizeBytes,
        FormatFileSize(asset.FileSizeBytes),
        asset.Width,
        asset.Height,
        asset.Url,
        asset.AltText,
        asset.Title,
        asset.Category,
        asset.CreatedAt,
        asset.UpdatedAt,
        asset.Usages?.Select(MapUsageResponse).ToList() ?? []
    );

    private static MediaUsageResponse MapUsageResponse(MediaUsage usage) => new(
        usage.Id,
        usage.EntityType,
        usage.EntityId,
        usage.FieldName,
        usage.CreatedAt
    );

    private static string FormatFileSize(long bytes) => bytes switch
    {
        < 1024 => $"{bytes} B",
        < 1024 * 1024 => $"{bytes / 1024.0:F1} KB",
        < 1024 * 1024 * 1024 => $"{bytes / (1024.0 * 1024):F1} MB",
        _ => $"{bytes / (1024.0 * 1024 * 1024):F1} GB"
    };

    private static string SanitizeFileName(string name)
    {
        // Replace spaces and special chars with hyphens, keep alphanumeric
        var sanitized = System.Text.RegularExpressions.Regex.Replace(
            name.Trim(), @"[^a-zA-Z0-9\-_]", "-");
        // Collapse multiple hyphens
        sanitized = System.Text.RegularExpressions.Regex.Replace(sanitized, @"-{2,}", "-");
        return sanitized.Trim('-').ToLowerInvariant();
    }

    /// <summary>
    /// Reads the width and height from raster image files by checking header bytes.
    /// Supports JPEG, PNG, GIF, BMP, and WebP.
    /// </summary>
    private static (int? width, int? height) GetImageDimensions(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        using var reader = new BinaryReader(stream);

        // Read first 30 bytes to detect format
        var header = reader.ReadBytes(30);
        if (header.Length < 8) return (null, null);

        // PNG: bytes 16-19 = width, 20-23 = height (big-endian)
        if (header[0] == 0x89 && header[1] == 0x50) // PNG
        {
            if (header.Length >= 24)
            {
                var w = (header[16] << 24) | (header[17] << 16) | (header[18] << 8) | header[19];
                var h = (header[20] << 24) | (header[21] << 16) | (header[22] << 8) | header[23];
                return (w, h);
            }
        }
        // GIF: bytes 6-7 = width, 8-9 = height (little-endian)
        else if (header[0] == 0x47 && header[1] == 0x49) // GIF
        {
            var w = header[6] | (header[7] << 8);
            var h = header[8] | (header[9] << 8);
            return (w, h);
        }
        // BMP: bytes 18-21 = width, 22-25 = height (little-endian)
        else if (header[0] == 0x42 && header[1] == 0x4D) // BMP
        {
            if (header.Length >= 26)
            {
                var w = header[18] | (header[19] << 8) | (header[20] << 16) | (header[21] << 24);
                var h = header[22] | (header[23] << 8) | (header[24] << 16) | (header[25] << 24);
                return (w, Math.Abs(h)); // height can be negative
            }
        }
        // JPEG: Need to scan for SOF markers
        else if (header[0] == 0xFF && header[1] == 0xD8) // JPEG
        {
            stream.Seek(2, SeekOrigin.Begin);
            while (stream.Position < stream.Length - 8)
            {
                int marker = stream.ReadByte();
                if (marker != 0xFF) continue;

                marker = stream.ReadByte();
                // SOF markers: C0-C3, C5-C7, C9-CB, CD-CF
                if ((marker >= 0xC0 && marker <= 0xC3) ||
                    (marker >= 0xC5 && marker <= 0xC7) ||
                    (marker >= 0xC9 && marker <= 0xCB) ||
                    (marker >= 0xCD && marker <= 0xCF))
                {
                    var buf = new byte[7];
                    if (stream.Read(buf, 0, 7) == 7)
                    {
                        var h = (buf[3] << 8) | buf[4];
                        var w = (buf[5] << 8) | buf[6];
                        return (w, h);
                    }
                }
                else if (marker == 0xD9 || marker == 0xDA) break; // EOI or SOS
                else
                {
                    // Skip segment
                    var lenBuf = new byte[2];
                    if (stream.Read(lenBuf, 0, 2) != 2) break;
                    var segLen = (lenBuf[0] << 8) | lenBuf[1];
                    stream.Seek(segLen - 2, SeekOrigin.Current);
                }
            }
        }

        return (null, null);
    }
}
