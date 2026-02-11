using eShopServer.Models;

namespace eShopServer.Interfaces.Repositories;

public interface IProductAttributeValueRepository : IRepository<ProductAttributeValue>
{
    Task<IEnumerable<ProductAttributeValue>> GetByProductIdAsync(int productId);
    Task DeleteByProductIdAsync(int productId);
}
