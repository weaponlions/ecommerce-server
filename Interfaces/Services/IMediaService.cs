using eShopServer.DTOs;
using Microsoft.AspNetCore.Http;

namespace eShopServer.Interfaces.Services;

public interface IMediaService
{
    // ── Upload & CRUD ──
    Task<MediaAssetResponse> UploadAsync(IFormFile file, string? altText, string? title, string category);
    Task<MediaAssetDetailResponse?> GetByIdAsync(int id);
    Task<PagedResponse<MediaAssetResponse>> GetAllAsync(string? search, string? category, int page, int pageSize);
    Task<MediaAssetResponse?> UpdateMetadataAsync(int id, UpdateMediaMetadataRequest request);
    Task<bool> DeleteAsync(int id);

    // ── Usage Tracking ──
    Task<MediaUsageResponse?> LinkAsync(LinkMediaRequest request);
    Task<bool> UnlinkAsync(int usageId);
    Task<IEnumerable<MediaUsageResponse>> GetUsagesForAssetAsync(int mediaAssetId);
    Task<IEnumerable<MediaAssetResponse>> GetMediaForEntityAsync(string entityType, int entityId);
}
