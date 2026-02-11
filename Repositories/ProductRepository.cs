using eShopServer.Data;
using eShopServer.Interfaces.Repositories;
using eShopServer.Models;
using Microsoft.EntityFrameworkCore;

namespace eShopServer.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Product>> GetTrendingAsync(int limit)
        => await _dbSet
            .Where(p => p.IsVisible)
            .OrderByDescending(p => p.TrendingScore)
            .Take(limit)
            .ToListAsync();
}
