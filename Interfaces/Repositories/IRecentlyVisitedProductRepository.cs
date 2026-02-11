using eShopServer.Models;

namespace eShopServer.Interfaces.Repositories;

public interface IRecentlyVisitedProductRepository : IRepository<RecentlyVisitedProduct>
{
    Task<IEnumerable<RecentlyVisitedProduct>> GetByUserAsync(string userId, int limit);
    Task<RecentlyVisitedProduct?> FindByUserAndProductAsync(string userId, int productId);
    Task RemoveOldestVisitsAsync(string userId, int keepCount);
}
