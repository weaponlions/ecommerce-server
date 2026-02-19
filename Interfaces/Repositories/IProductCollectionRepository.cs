using eShopServer.Models;

namespace eShopServer.Interfaces.Repositories;

public interface IProductCollectionRepository
{
    Task<IEnumerable<ProductCollection>> GetByCollectionIdAsync(int collectionId);
    Task<IEnumerable<ProductCollection>> GetByProductIdAsync(int productId);
    Task AddAsync(ProductCollection entity);
    Task RemoveAsync(int productId, int collectionId);
    Task RemoveAllByCollectionIdAsync(int collectionId);
    Task RemoveAllByProductIdAsync(int productId);
}
