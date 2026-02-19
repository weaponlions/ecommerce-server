using eShopServer.Data;
using eShopServer.Interfaces.Repositories;
using eShopServer.Models;
using Microsoft.EntityFrameworkCore;

namespace eShopServer.Repositories;

public class ProductCollectionRepository : IProductCollectionRepository
{
    private readonly AppDbContext _context;

    public ProductCollectionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProductCollection>> GetByCollectionIdAsync(int collectionId)
        => await _context.ProductCollections
            .Where(pc => pc.CollectionId == collectionId)
            .Include(pc => pc.Product)
            .OrderBy(pc => pc.DisplayOrder)
            .ToListAsync();

    public async Task<IEnumerable<ProductCollection>> GetByProductIdAsync(int productId)
        => await _context.ProductCollections
            .Where(pc => pc.ProductId == productId)
            .Include(pc => pc.Collection)
            .ToListAsync();

    public async Task AddAsync(ProductCollection entity)
    {
        await _context.ProductCollections.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveAsync(int productId, int collectionId)
    {
        var entity = await _context.ProductCollections
            .FirstOrDefaultAsync(pc => pc.ProductId == productId && pc.CollectionId == collectionId);
        if (entity != null)
        {
            _context.ProductCollections.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task RemoveAllByCollectionIdAsync(int collectionId)
    {
        var entries = await _context.ProductCollections
            .Where(pc => pc.CollectionId == collectionId).ToListAsync();
        _context.ProductCollections.RemoveRange(entries);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveAllByProductIdAsync(int productId)
    {
        var entries = await _context.ProductCollections
            .Where(pc => pc.ProductId == productId).ToListAsync();
        _context.ProductCollections.RemoveRange(entries);
        await _context.SaveChangesAsync();
    }
}
