using eShopServer.Data;
using eShopServer.Interfaces.Repositories;
using eShopServer.Models;
using Microsoft.EntityFrameworkCore;

namespace eShopServer.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(AppDbContext context) : base(context) { }

    public async Task<Category?> GetBySlugAsync(string slug)
        => await _dbSet
            .FirstOrDefaultAsync(c => c.Slug == slug.ToLowerInvariant());

    public async Task<Category?> GetWithAttributesAsync(int id)
        => await _dbSet
            .Include(c => c.Attributes.OrderBy(a => a.DisplayOrder))
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<IEnumerable<Category>> GetActiveCategoriesAsync()
        => await _dbSet
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
}
