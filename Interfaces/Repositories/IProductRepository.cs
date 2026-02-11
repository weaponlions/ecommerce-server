using eShopServer.Models;

namespace eShopServer.Interfaces.Repositories;

public interface IProductRepository : IRepository<Product>
{
    Task<IEnumerable<Product>> GetTrendingAsync(int limit);
}
