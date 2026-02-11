using eShopServer.Models;

namespace eShopServer.Interfaces.Repositories;

public interface IMediaUsageRepository : IRepository<MediaUsage>
{
    Task<IEnumerable<MediaUsage>> GetByMediaAssetIdAsync(int mediaAssetId);
    Task<IEnumerable<MediaUsage>> GetByEntityAsync(string entityType, int entityId);
    Task<MediaUsage?> FindExactAsync(int mediaAssetId, string entityType, int entityId, string fieldName);
    Task DeleteByMediaAssetIdAsync(int mediaAssetId);
    Task DeleteByEntityAsync(string entityType, int entityId);
}
