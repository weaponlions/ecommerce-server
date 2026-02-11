using eShopServer.Data;
using eShopServer.Interfaces.Repositories;
using eShopServer.Models;
using Microsoft.EntityFrameworkCore;

namespace eShopServer.Repositories;

public class MediaUsageRepository : Repository<MediaUsage>, IMediaUsageRepository
{
    public MediaUsageRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<MediaUsage>> GetByMediaAssetIdAsync(int mediaAssetId)
        => await _dbSet
            .Where(u => u.MediaAssetId == mediaAssetId)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<MediaUsage>> GetByEntityAsync(string entityType, int entityId)
        => await _dbSet
            .Include(u => u.MediaAsset)
            .Where(u => u.EntityType == entityType && u.EntityId == entityId)
            .ToListAsync();

    public async Task<MediaUsage?> FindExactAsync(
        int mediaAssetId, string entityType, int entityId, string fieldName)
        => await _dbSet.FirstOrDefaultAsync(u =>
            u.MediaAssetId == mediaAssetId &&
            u.EntityType == entityType &&
            u.EntityId == entityId &&
            u.FieldName == fieldName);

    public async Task DeleteByMediaAssetIdAsync(int mediaAssetId)
    {
        var usages = await _dbSet.Where(u => u.MediaAssetId == mediaAssetId).ToListAsync();
        _dbSet.RemoveRange(usages);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteByEntityAsync(string entityType, int entityId)
    {
        var usages = await _dbSet
            .Where(u => u.EntityType == entityType && u.EntityId == entityId)
            .ToListAsync();
        _dbSet.RemoveRange(usages);
        await _context.SaveChangesAsync();
    }
}
