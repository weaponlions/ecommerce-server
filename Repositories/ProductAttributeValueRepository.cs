using eShopServer.Data;
using eShopServer.Interfaces.Repositories;
using eShopServer.Models;
using Microsoft.EntityFrameworkCore;

namespace eShopServer.Repositories;

public class ProductAttributeValueRepository : Repository<ProductAttributeValue>, IProductAttributeValueRepository
{
    public ProductAttributeValueRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<ProductAttributeValue>> GetByProductIdAsync(int productId)
        => await _dbSet
            .Where(v => v.ProductId == productId)
            .Include(v => v.CategoryAttribute)
            .ToListAsync();

    public async Task DeleteByProductIdAsync(int productId)
    {
        var values = await _dbSet.Where(v => v.ProductId == productId).ToListAsync();
        _dbSet.RemoveRange(values);
        await _context.SaveChangesAsync();
    }
}
