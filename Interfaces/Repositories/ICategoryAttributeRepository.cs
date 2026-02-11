using eShopServer.Models;

namespace eShopServer.Interfaces.Repositories;

public interface ICategoryAttributeRepository : IRepository<CategoryAttribute>
{
    Task<IEnumerable<CategoryAttribute>> GetByCategoryIdAsync(int categoryId);
    Task<CategoryAttribute?> GetByNameAsync(int categoryId, string name);
}
