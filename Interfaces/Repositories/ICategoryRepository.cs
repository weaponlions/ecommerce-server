using eShopServer.Models;

namespace eShopServer.Interfaces.Repositories;

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetBySlugAsync(string slug);
    Task<Category?> GetWithAttributesAsync(int id);
    Task<IEnumerable<Category>> GetActiveCategoriesAsync();
}
