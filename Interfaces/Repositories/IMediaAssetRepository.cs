using eShopServer.Models;

namespace eShopServer.Interfaces.Repositories;

public interface IMediaAssetRepository : IRepository<MediaAsset>
{
    Task<IEnumerable<MediaAsset>> GetByCategoryAsync(string category);
    Task<MediaAsset?> GetByFileNameAsync(string fileName);
    Task<MediaAsset?> GetWithUsagesAsync(int id);
    Task<IEnumerable<MediaAsset>> SearchAsync(string? search, string? category, int page, int pageSize);
    Task<int> GetTotalCountAsync(string? search, string? category);
}
