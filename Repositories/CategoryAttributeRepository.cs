using eShopServer.Data;
using eShopServer.Interfaces.Repositories;
using eShopServer.Models;
using Microsoft.EntityFrameworkCore;

namespace eShopServer.Repositories;

public class CategoryAttributeRepository : Repository<CategoryAttribute>, ICategoryAttributeRepository
{
    public CategoryAttributeRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<CategoryAttribute>> GetByCategoryIdAsync(int categoryId)
        => await _dbSet
            .Where(a => a.CategoryId == categoryId)
            .OrderBy(a => a.DisplayOrder)
            .ToListAsync();

    public async Task<CategoryAttribute?> GetByNameAsync(int categoryId, string name)
        => await _dbSet
            .FirstOrDefaultAsync(a => a.CategoryId == categoryId && a.Name == name.ToLowerInvariant());
}
