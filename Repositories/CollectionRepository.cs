using eShopServer.Data;
using eShopServer.Interfaces.Repositories;
using eShopServer.Models;
using Microsoft.EntityFrameworkCore;

namespace eShopServer.Repositories;

public class CollectionRepository : Repository<Collection>, ICollectionRepository
{
    public CollectionRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Collection>> GetMostVisitedAsync(int limit)
        => await _dbSet
            .Where(c => c.IsVisible)
            .OrderByDescending(c => c.VisitCount)
            .Take(limit)
            .ToListAsync();
}
