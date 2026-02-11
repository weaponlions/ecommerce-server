using eShopServer.Data;
using eShopServer.Interfaces.Repositories;
using eShopServer.Models;
using Microsoft.EntityFrameworkCore;

namespace eShopServer.Repositories;

public class RecentlyVisitedProductRepository : Repository<RecentlyVisitedProduct>, IRecentlyVisitedProductRepository
{
    public RecentlyVisitedProductRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<RecentlyVisitedProduct>> GetByUserAsync(string userId, int limit)
        => await _dbSet
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.VisitedAt)
            .Take(limit)
            .Include(r => r.Product)
            .Where(r => r.Product.IsVisible)
            .ToListAsync();

    public async Task<RecentlyVisitedProduct?> FindByUserAndProductAsync(string userId, int productId)
        => await _dbSet
            .FirstOrDefaultAsync(r => r.UserId == userId && r.ProductId == productId);

    public async Task RemoveOldestVisitsAsync(string userId, int keepCount)
    {
        var userVisits = await _dbSet
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.VisitedAt)
            .ToListAsync();

        if (userVisits.Count > keepCount)
        {
            var toRemove = userVisits.Skip(keepCount);
            _dbSet.RemoveRange(toRemove);
            await _context.SaveChangesAsync();
        }
    }
}
