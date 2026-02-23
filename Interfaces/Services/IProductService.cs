using eShopServer.DTOs;
using eShopServer.Models;

namespace eShopServer.Interfaces.Services;

public interface IProductService
{
    // ── Categories ──
    Task<IEnumerable<CategoryListResponse>> GetActiveCategoriesAsync();
    Task<CategoryResponse?> GetCategoryBySlugAsync(string slug);
    Task<CategoryResponse?> GetCategoryByIdAsync(int id);
    Task<Category> CreateCategoryAsync(UpsertCategoryRequest request);
    Task<Category?> UpdateCategoryAsync(int id, UpsertCategoryRequest request);
    Task<bool> DeleteCategoryAsync(int id);

    // ── Category Attributes ──
    Task<IEnumerable<CategoryAttributeResponse>> GetCategoryAttributesAsync(int categoryId);
    Task<CategoryAttribute> CreateCategoryAttributeAsync(int categoryId, UpsertCategoryAttributeRequest request);
    Task<CategoryAttribute?> UpdateCategoryAttributeAsync(int attributeId, UpsertCategoryAttributeRequest request);
    Task<bool> DeleteCategoryAttributeAsync(int attributeId);

    // ── Products ──
    Task<ProductDetailResponse?> GetProductByIdAsync(int id);
    Task<PagedResponse<ProductListItemResponse>> GetProductsAsync(ProductFilterRequest filter);
    Task<ProductDetailResponse> CreateProductAsync(CreateProductRequest request);
    Task<ProductDetailResponse?> UpdateProductAsync(int id, UpdateProductRequest request);
    Task<bool> DeleteProductAsync(int id);

    // ── Collections ──
    Task<IEnumerable<CollectionDto>> GetActiveCollectionsAsync();
    Task<CollectionDto?> GetCollectionByIdAsync(int id);

    // ── Collection ↔ Product ──
    Task<IEnumerable<CollectionProductResponse>> GetCollectionProductsAsync(int collectionId);
    Task AddProductToCollectionAsync(int collectionId, AddProductToCollectionRequest request);
    Task<bool> RemoveProductFromCollectionAsync(int collectionId, int productId);

    // ── Variants ──
    Task<IEnumerable<VariantSummary>> GetVariantSiblingsAsync(int productId);
}
