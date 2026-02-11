using eShopServer.Models;

namespace eShopServer.Interfaces.Repositories;

public interface ICollectionRepository : IRepository<Collection>
{
    Task<IEnumerable<Collection>> GetMostVisitedAsync(int limit);
}
