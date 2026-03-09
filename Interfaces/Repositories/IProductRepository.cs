using eShopServer.Models;

namespace eShopServer.Interfaces.Repositories;

public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetBySlugAsync(string slug);
    Task<IEnumerable<Product>> GetTrendingAsync(int limit);
}
