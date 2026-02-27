using eShopServer.Data;
using eShopServer.Interfaces.Repositories;
using eShopServer.Models;
using Microsoft.EntityFrameworkCore;

namespace eShopServer.Repositories;

public class ProductImageRepository : Repository<ProductImage>, IProductImageRepository
{
    public ProductImageRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<ProductImage>> GetByProductIdAsync(int productId)
        => await _dbSet
            .Include(pi => pi.MediaAsset)
            .Where(pi => pi.ProductId == productId)
            .OrderBy(pi => pi.DisplayOrder)
            .ToListAsync();

    public async Task<ProductImage?> GetByProductIdAndMediaQueryAsync(int productId, int mediaAssetId)
        => await _dbSet
            .Include(pi => pi.MediaAsset)
            .FirstOrDefaultAsync(pi => pi.ProductId == productId && pi.MediaAssetId == mediaAssetId);
}
