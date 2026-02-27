using eShopServer.Models;

namespace eShopServer.Interfaces.Repositories;

public interface IProductImageRepository : IRepository<ProductImage>
{
    Task<IEnumerable<ProductImage>> GetByProductIdAsync(int productId);
    Task<ProductImage?> GetByProductIdAndMediaQueryAsync(int productId, int mediaAssetId);
}
