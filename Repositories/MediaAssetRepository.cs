using eShopServer.Data;
using eShopServer.Interfaces.Repositories;
using eShopServer.Models;
using Microsoft.EntityFrameworkCore;

namespace eShopServer.Repositories;

public class MediaAssetRepository : Repository<MediaAsset>, IMediaAssetRepository
{
    public MediaAssetRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<MediaAsset>> GetByCategoryAsync(string category)
        => await _dbSet
            .Where(m => m.Category == category)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();

    public async Task<MediaAsset?> GetByFileNameAsync(string fileName)
        => await _dbSet.FirstOrDefaultAsync(m => m.FileName == fileName);

    public async Task<MediaAsset?> GetWithUsagesAsync(int id)
        => await _dbSet
            .Include(m => m.Usages)
            .FirstOrDefaultAsync(m => m.Id == id);

    public async Task<IEnumerable<MediaAsset>> SearchAsync(
        string? search, string? category, int page, int pageSize)
    {
        var query = _dbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(m => m.Category == category);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(m =>
                m.OriginalFileName.Contains(search) ||
                (m.Title != null && m.Title.Contains(search)) ||
                (m.AltText != null && m.AltText.Contains(search)));

        return await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(m => m.Usages)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync(string? search, string? category)
    {
        var query = _dbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(m => m.Category == category);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(m =>
                m.OriginalFileName.Contains(search) ||
                (m.Title != null && m.Title.Contains(search)) ||
                (m.AltText != null && m.AltText.Contains(search)));

        return await query.CountAsync();
    }
}
