using System.Text.Json;
using eShopServer.DTOs;
using eShopServer.Interfaces.Repositories;
using eShopServer.Interfaces.Services;
using eShopServer.Models;

namespace eShopServer.Services;

public class ProductService : IProductService
{
    private readonly ICategoryRepository _categoryRepo;
    private readonly ICategoryAttributeRepository _attrRepo;
    private readonly IProductRepository _productRepo;
    private readonly IProductAttributeValueRepository _attrValueRepo;
    private readonly IMediaAssetRepository _mediaAssetRepo;
    private readonly IMediaUsageRepository _mediaUsageRepo;
    private readonly IProductCollectionRepository _productCollectionRepo;
    private readonly ICollectionRepository _collectionRepo;

    public ProductService(
        ICategoryRepository categoryRepo,
        ICategoryAttributeRepository attrRepo,
        IProductRepository productRepo,
        IProductAttributeValueRepository attrValueRepo,
        IMediaAssetRepository mediaAssetRepo,
        IMediaUsageRepository mediaUsageRepo,
        IProductCollectionRepository productCollectionRepo,
        ICollectionRepository collectionRepo)
    {
        _categoryRepo = categoryRepo;
        _attrRepo = attrRepo;
        _productRepo = productRepo;
        _attrValueRepo = attrValueRepo;
        _mediaAssetRepo = mediaAssetRepo;
        _mediaUsageRepo = mediaUsageRepo;
        _productCollectionRepo = productCollectionRepo;
        _collectionRepo = collectionRepo;
    }

    // ════════════════════════════════════════════════════════════════
    //  Categories
    // ════════════════════════════════════════════════════════════════

    public async Task<IEnumerable<CategoryListResponse>> GetActiveCategoriesAsync()
    {
        var categories = await _categoryRepo.GetActiveCategoriesAsync();
        return categories.Select(c => new CategoryListResponse(
            c.Id, c.Name, c.Slug, c.Description, c.MediaAssetId
        ));
    }

    public async Task<CategoryResponse?> GetCategoryBySlugAsync(string slug)
    {
        var category = await _categoryRepo.GetBySlugAsync(slug);
        if (category is null) return null;

        var withAttrs = await _categoryRepo.GetWithAttributesAsync(category.Id);
        return MapCategoryResponse(withAttrs!);
    }

    public async Task<CategoryResponse?> GetCategoryByIdAsync(int id)
    {
        var category = await _categoryRepo.GetWithAttributesAsync(id);
        return category is null ? null : MapCategoryResponse(category);
    }

    public async Task<Category> CreateCategoryAsync(UpsertCategoryRequest request)
    {
        // Validate the media asset if provided
        if (request.MediaAssetId.HasValue)
            await ValidateMediaAsset(request.MediaAssetId.Value);

        var category = new Category(request.Name, request.Slug)
        {
            Description = request.Description,
            MediaAssetId = request.MediaAssetId,
            IsActive = request.IsActive
        };

        var created = await _categoryRepo.AddAsync(category);

        if (request.MediaAssetId.HasValue)
            await TrackMediaUsage(request.MediaAssetId.Value, "Category", created.Id, "MediaAssetId");

        return created;
    }

    public async Task<Category?> UpdateCategoryAsync(int id, UpsertCategoryRequest request)
    {
        var category = await _categoryRepo.GetByIdAsync(id);
        if (category is null) return null;

        // Validate the media asset if provided
        if (request.MediaAssetId.HasValue)
            await ValidateMediaAsset(request.MediaAssetId.Value);

        category.Name = request.Name;
        category.Slug = request.Slug.ToLowerInvariant();
        category.Description = request.Description;
        category.MediaAssetId = request.MediaAssetId;
        category.IsActive = request.IsActive;

        var updated = await _categoryRepo.UpdateAsync(category);

        if (request.MediaAssetId.HasValue)
            await TrackMediaUsage(request.MediaAssetId.Value, "Category", id, "MediaAssetId");

        return updated;
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        await _mediaUsageRepo.DeleteByEntityAsync("Category", id);
        return await _categoryRepo.DeleteAsync(id);
    }

    // ════════════════════════════════════════════════════════════════
    //  Category Attributes
    // ════════════════════════════════════════════════════════════════

    public async Task<IEnumerable<CategoryAttributeResponse>> GetCategoryAttributesAsync(int categoryId)
    {
        var attrs = await _attrRepo.GetByCategoryIdAsync(categoryId);
        return attrs.Select(MapAttributeResponse);
    }

    public async Task<CategoryAttribute> CreateCategoryAttributeAsync(
        int categoryId, UpsertCategoryAttributeRequest request)
    {
        var dataType = ParseDataType(request.DataType);
        var attr = new CategoryAttribute(categoryId, request.Name, request.DisplayName, dataType)
        {
            IsRequired = request.IsRequired,
            IsFilterable = request.IsFilterable,
            Options = request.Options != null ? JsonSerializer.Serialize(request.Options) : null,
            DisplayOrder = request.DisplayOrder
        };
        return await _attrRepo.AddAsync(attr);
    }

    public async Task<CategoryAttribute?> UpdateCategoryAttributeAsync(
        int attributeId, UpsertCategoryAttributeRequest request)
    {
        var attr = await _attrRepo.GetByIdAsync(attributeId);
        if (attr is null) return null;

        var dataType = ParseDataType(request.DataType);
        attr.Name = request.Name.ToLowerInvariant();
        attr.DisplayName = request.DisplayName;
        attr.DataType = dataType;
        attr.IsRequired = request.IsRequired;
        attr.IsFilterable = request.IsFilterable;
        attr.Options = request.Options != null ? JsonSerializer.Serialize(request.Options) : null;
        attr.DisplayOrder = request.DisplayOrder;

        return await _attrRepo.UpdateAsync(attr);
    }

    public async Task<bool> DeleteCategoryAttributeAsync(int attributeId)
        => await _attrRepo.DeleteAsync(attributeId);

    // ════════════════════════════════════════════════════════════════
    //  Products
    // ════════════════════════════════════════════════════════════════

    public async Task<ProductDetailResponse?> GetProductByIdAsync(int id)
    {
        var product = await _productRepo.GetByIdAsync(id);
        if (product is null) return null;
        return await BuildProductDetail(product);
    }

    public async Task<PagedResponse<ProductListItemResponse>> GetProductsAsync(ProductFilterRequest filter)
    {
        var allProducts = (await _productRepo.GetAllAsync())
            .Where(p => p.IsVisible);

        // ── Category filter ──
        if (filter.CategoryId.HasValue)
            allProducts = allProducts.Where(p => p.CategoryId == filter.CategoryId.Value);
        else if (!string.IsNullOrWhiteSpace(filter.CategorySlug))
        {
            var cat = await _categoryRepo.GetBySlugAsync(filter.CategorySlug);
            if (cat != null)
                allProducts = allProducts.Where(p => p.CategoryId == cat.Id);
            else
                allProducts = Enumerable.Empty<Product>();
        }

        // ── Collection filter ──
        if (filter.CollectionId.HasValue)
        {
            var collectionProducts = await _productCollectionRepo.GetByCollectionIdAsync(filter.CollectionId.Value);
            var collectionProductIds = collectionProducts.Select(pc => pc.ProductId).ToHashSet();
            allProducts = allProducts.Where(p => collectionProductIds.Contains(p.Id));
        }

        // ── Price filter ──
        if (filter.MinPrice.HasValue)
            allProducts = allProducts.Where(p => p.Price >= filter.MinPrice.Value);
        if (filter.MaxPrice.HasValue)
            allProducts = allProducts.Where(p => p.Price <= filter.MaxPrice.Value);

        // ── Text search (token-based) ──
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var tokens = filter.Search
                .ToLowerInvariant()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            allProducts = allProducts.Where(p =>
                tokens.All(token =>
                    p.Name.Contains(token, StringComparison.OrdinalIgnoreCase) ||
                    (p.Description != null && p.Description.Contains(token, StringComparison.OrdinalIgnoreCase)) ||
                    (p.CategoryLabel != null && p.CategoryLabel.Contains(token, StringComparison.OrdinalIgnoreCase)) ||
                    (p.Badge != null && p.Badge.Contains(token, StringComparison.OrdinalIgnoreCase))
                ));
        }

        var productList = allProducts.ToList();

        // ── Attribute filter ──
        if (filter.Attributes != null && filter.Attributes.Count > 0)
        {
            var filtered = new List<Product>();
            foreach (var product in productList)
            {
                var attrValues = await _attrValueRepo.GetByProductIdAsync(product.Id);
                var match = true;
                foreach (var (key, value) in filter.Attributes)
                {
                    var attrVal = attrValues.FirstOrDefault(
                        v => v.CategoryAttribute.Name == key.ToLowerInvariant());
                    if (attrVal is null || !AttributeValueMatches(attrVal.Value, value))
                    {
                        match = false;
                        break;
                    }
                }
                if (match) filtered.Add(product);
            }
            productList = filtered;
        }

        // ── Sorting ──
        productList = (filter.SortBy?.ToLowerInvariant()) switch
        {
            "price" => filter.SortDescending
                ? productList.OrderByDescending(p => p.Price).ToList()
                : productList.OrderBy(p => p.Price).ToList(),
            "name" => filter.SortDescending
                ? productList.OrderByDescending(p => p.Name).ToList()
                : productList.OrderBy(p => p.Name).ToList(),
            "rating" => filter.SortDescending
                ? productList.OrderByDescending(p => p.Rating).ToList()
                : productList.OrderBy(p => p.Rating).ToList(),
            "newest" => productList.OrderByDescending(p => p.CreatedAt).ToList(),
            _ => productList.OrderByDescending(p => p.TrendingScore).ToList()
        };

        // ── Pagination ──
        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);
        var totalCount = productList.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var items = productList
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductListItemResponse(
                p.Id, p.Name, p.Price, p.OriginalPrice,
                p.MediaAssetId, p.CategoryLabel, p.Badge, p.Rating, p.ReviewCount, p.Stock,
                p.VariantGroupId
            )).ToList();

        return new PagedResponse<ProductListItemResponse>(items, totalCount, page, pageSize, totalPages);
    }

    public async Task<ProductDetailResponse> CreateProductAsync(CreateProductRequest req)
    {
        // Validate the media asset exists
        await ValidateMediaAsset(req.MediaAssetId);

        var product = new Product(req.Name, req.Price)
        {
            Description = req.Description,
            OriginalPrice = req.OriginalPrice,
            MediaAssetId = req.MediaAssetId,
            CategoryLabel = req.CategoryLabel,
            Badge = req.Badge,
            Rating = req.Rating,
            ReviewCount = req.ReviewCount,
            TrendingScore = req.TrendingScore,
            IsVisible = req.IsVisible,
            CategoryId = req.CategoryId,
            Stock = req.Stock,
            VariantGroupId = req.VariantGroupId
        };

        var created = await _productRepo.AddAsync(product);
        await TrackMediaUsage(req.MediaAssetId, "Product", created.Id, "MediaAssetId");

        // ── Save attribute values ──
        if (req.CategoryId.HasValue && req.Attributes != null)
        {
            await SaveAttributeValues(created.Id, req.CategoryId.Value, req.Attributes);
        }

        return (await BuildProductDetail(created))!;
    }

    public async Task<ProductDetailResponse?> UpdateProductAsync(int id, UpdateProductRequest req)
    {
        var product = await _productRepo.GetByIdAsync(id);
        if (product is null) return null;

        // Validate the media asset exists
        await ValidateMediaAsset(req.MediaAssetId);

        product.Name = req.Name;
        product.Description = req.Description;
        product.Price = req.Price;
        product.OriginalPrice = req.OriginalPrice;
        product.MediaAssetId = req.MediaAssetId;
        product.CategoryLabel = req.CategoryLabel;
        product.Badge = req.Badge;
        product.Rating = req.Rating;
        product.ReviewCount = req.ReviewCount;
        product.TrendingScore = req.TrendingScore;
        product.IsVisible = req.IsVisible;
        product.Stock = req.Stock;
        product.VariantGroupId = req.VariantGroupId;

        // ── If category changed, clear old attribute values ──
        if (product.CategoryId != req.CategoryId)
        {
            await _attrValueRepo.DeleteByProductIdAsync(id);
            product.CategoryId = req.CategoryId;
        }

        await _productRepo.UpdateAsync(product);
        await TrackMediaUsage(req.MediaAssetId, "Product", id, "MediaAssetId");

        // ── Save new attribute values ──
        if (req.CategoryId.HasValue && req.Attributes != null)
        {
            await _attrValueRepo.DeleteByProductIdAsync(id);
            await SaveAttributeValues(id, req.CategoryId.Value, req.Attributes);
        }

        return await BuildProductDetail(product);
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        await _productCollectionRepo.RemoveAllByProductIdAsync(id);
        await _mediaUsageRepo.DeleteByEntityAsync("Product", id);
        return await _productRepo.DeleteAsync(id);
    }

    // ════════════════════════════════════════════════════════════════
    //  Collections
    // ════════════════════════════════════════════════════════════════

    public async Task<IEnumerable<CollectionDto>> GetActiveCollectionsAsync()
    {
        var collections = (await _collectionRepo.GetAllAsync())
            .Where(c => c.IsVisible)
            .OrderBy(c => c.DisplayOrder);
        return collections.Select(c => new CollectionDto(
            c.Id, c.Name, c.Description, c.MediaAssetId, c.LinkUrl, c.VisitCount
        ));
    }

    public async Task<CollectionDto?> GetCollectionByIdAsync(int id)
    {
        var collection = await _collectionRepo.GetByIdAsync(id);
        if (collection is null || !collection.IsVisible) return null;
        return new CollectionDto(
            collection.Id, collection.Name, collection.Description,
            collection.MediaAssetId, collection.LinkUrl, collection.VisitCount
        );
    }

    // ════════════════════════════════════════════════════════════════
    //  Collection ↔ Product
    // ════════════════════════════════════════════════════════════════

    public async Task<IEnumerable<CollectionProductResponse>> GetCollectionProductsAsync(int collectionId)
    {
        var entries = await _productCollectionRepo.GetByCollectionIdAsync(collectionId);
        return entries.Select(pc => new CollectionProductResponse(
            pc.ProductId,
            pc.Product.Name,
            pc.Product.Price,
            pc.Product.MediaAssetId,
            pc.DisplayOrder
        ));
    }

    public async Task AddProductToCollectionAsync(int collectionId, AddProductToCollectionRequest request)
    {
        var pc = new ProductCollection
        {
            ProductId = request.ProductId,
            CollectionId = collectionId,
            DisplayOrder = request.DisplayOrder
        };
        await _productCollectionRepo.AddAsync(pc);
    }

    public async Task<bool> RemoveProductFromCollectionAsync(int collectionId, int productId)
    {
        var entries = await _productCollectionRepo.GetByCollectionIdAsync(collectionId);
        var exists = entries.Any(pc => pc.ProductId == productId);
        if (!exists) return false;

        await _productCollectionRepo.RemoveAsync(productId, collectionId);
        return true;
    }

    // ════════════════════════════════════════════════════════════════
    //  Variants
    // ════════════════════════════════════════════════════════════════

    public async Task<IEnumerable<VariantSummary>> GetVariantSiblingsAsync(int productId)
    {
        var product = await _productRepo.GetByIdAsync(productId);
        if (product is null || string.IsNullOrWhiteSpace(product.VariantGroupId))
            return Enumerable.Empty<VariantSummary>();

        var siblings = (await _productRepo.FindAsync(
            p => p.VariantGroupId == product.VariantGroupId && p.Id != productId && p.IsVisible
        )).ToList();

        var result = new List<VariantSummary>();
        foreach (var sibling in siblings)
        {
            var attrValues = await _attrValueRepo.GetByProductIdAsync(sibling.Id);
            var attrs = attrValues.ToDictionary(
                v => v.CategoryAttribute.DisplayName,
                v => v.Value
            );
            result.Add(new VariantSummary(sibling.Id, sibling.Name, sibling.Price, sibling.MediaAssetId, attrs));
        }
        return result;
    }

    // ════════════════════════════════════════════════════════════════
    //  Private Helpers
    // ════════════════════════════════════════════════════════════════

    private CategoryResponse MapCategoryResponse(Category c)
    {
        return new CategoryResponse(
            c.Id, c.Name, c.Slug, c.Description, c.MediaAssetId, c.IsActive,
            c.Attributes?.Select(MapAttributeResponse).ToList()
        );
    }

    private CategoryAttributeResponse MapAttributeResponse(CategoryAttribute a)
    {
        string[]? options = null;
        if (a.Options != null)
        {
            try { options = JsonSerializer.Deserialize<string[]>(a.Options); }
            catch { options = null; }
        }

        return new CategoryAttributeResponse(
            a.Id, a.Name, a.DisplayName, a.DataType.ToString(),
            a.IsRequired, a.IsFilterable, options, a.DisplayOrder
        );
    }

    private async Task<ProductDetailResponse> BuildProductDetail(Product p)
    {
        var attrValues = await _attrValueRepo.GetByProductIdAsync(p.Id);
        var attributes = new Dictionary<string, object?>();

        foreach (var av in attrValues)
        {
            var key = av.CategoryAttribute.Name;
            object? value = av.CategoryAttribute.DataType switch
            {
                AttributeDataType.Number => decimal.TryParse(av.Value, out var n) ? n : av.Value,
                AttributeDataType.Boolean => av.Value.Equals("true", StringComparison.OrdinalIgnoreCase),
                AttributeDataType.MultiSelect => TryParseJsonArray(av.Value),
                _ => av.Value
            };
            attributes[key] = value;
        }

        // Get category info
        string? categoryName = null;
        string? categorySlug = null;
        if (p.CategoryId.HasValue)
        {
            var category = await _categoryRepo.GetByIdAsync(p.CategoryId.Value);
            if (category != null)
            {
                categoryName = category.Name;
                categorySlug = category.Slug;
            }
        }

        // Get variant siblings (if this product belongs to a variant group)
        List<VariantSummary>? variants = null;
        if (!string.IsNullOrWhiteSpace(p.VariantGroupId))
        {
            variants = (await GetVariantSiblingsAsync(p.Id)).ToList();
            if (variants.Count == 0) variants = null;
        }

        return new ProductDetailResponse(
            p.Id, p.Name, p.Description, p.Price, p.OriginalPrice,
            p.MediaAssetId, p.CategoryLabel, p.Badge, p.Rating, p.ReviewCount, p.Stock,
            p.CategoryId, categoryName, categorySlug, p.VariantGroupId, variants, attributes
        );
    }

    private async Task SaveAttributeValues(int productId, int categoryId, Dictionary<string, string> attributes)
    {
        var categoryAttrs = await _attrRepo.GetByCategoryIdAsync(categoryId);

        foreach (var catAttr in categoryAttrs)
        {
            if (attributes.TryGetValue(catAttr.Name, out var value) && !string.IsNullOrWhiteSpace(value))
            {
                ValidateAttributeValue(catAttr, value);
                var attrValue = new ProductAttributeValue(productId, catAttr.Id, value);
                await _attrValueRepo.AddAsync(attrValue);
            }
            else if (catAttr.IsRequired)
            {
                throw new ArgumentException(
                    $"Attribute '{catAttr.DisplayName}' is required for this category.");
            }
        }
    }

    /// <summary>
    /// Validates that a media asset with the given ID exists.
    /// </summary>
    private async Task ValidateMediaAsset(int mediaAssetId)
    {
        var asset = await _mediaAssetRepo.GetByIdAsync(mediaAssetId);
        if (asset is null)
            throw new ArgumentException(
                $"Media asset with ID {mediaAssetId} not found. Upload the image first via /api/admin/media/upload.");
    }

    /// <summary>
    /// Creates a MediaUsage record to track that an entity uses a specific media asset.
    /// Avoids creating duplicates.
    /// </summary>
    private async Task TrackMediaUsage(int mediaAssetId, string entityType, int entityId, string fieldName)
    {
        var existing = await _mediaUsageRepo.FindExactAsync(
            mediaAssetId, entityType, entityId, fieldName);
        if (existing != null) return;

        var usage = new MediaUsage
        {
            MediaAssetId = mediaAssetId,
            EntityType = entityType,
            EntityId = entityId,
            FieldName = fieldName
        };
        await _mediaUsageRepo.AddAsync(usage);
    }

    private static void ValidateAttributeValue(CategoryAttribute attr, string value)
    {
        switch (attr.DataType)
        {
            case AttributeDataType.Number:
                if (!decimal.TryParse(value, out _))
                    throw new ArgumentException(
                        $"Attribute '{attr.DisplayName}' must be a number. Got: '{value}'");
                break;

            case AttributeDataType.Boolean:
                if (!value.Equals("true", StringComparison.OrdinalIgnoreCase) &&
                    !value.Equals("false", StringComparison.OrdinalIgnoreCase))
                    throw new ArgumentException(
                        $"Attribute '{attr.DisplayName}' must be 'true' or 'false'. Got: '{value}'");
                break;

            case AttributeDataType.Select:
                if (attr.Options != null)
                {
                    var options = JsonSerializer.Deserialize<string[]>(attr.Options);
                    if (options != null && !options.Contains(value, StringComparer.OrdinalIgnoreCase))
                        throw new ArgumentException(
                            $"Attribute '{attr.DisplayName}' must be one of: [{string.Join(", ", options)}]. Got: '{value}'");
                }
                break;

            case AttributeDataType.MultiSelect:
                if (attr.Options != null)
                {
                    var allowed = JsonSerializer.Deserialize<string[]>(attr.Options);
                    var selected = TryParseJsonArray(value);
                    if (allowed != null && selected != null)
                    {
                        var invalid = selected.Except(allowed, StringComparer.OrdinalIgnoreCase).ToList();
                        if (invalid.Count > 0)
                            throw new ArgumentException(
                                $"Attribute '{attr.DisplayName}' contains invalid options: [{string.Join(", ", invalid)}]. Allowed: [{string.Join(", ", allowed)}]");
                    }
                }
                break;
        }
    }

    private static string[]? TryParseJsonArray(string value)
    {
        try { return JsonSerializer.Deserialize<string[]>(value); }
        catch { return null; }
    }

    private static bool AttributeValueMatches(string storedValue, string filterValue)
    {
        // For multi-select, check if any selected value matches
        var stored = TryParseJsonArray(storedValue);
        if (stored != null)
            return stored.Contains(filterValue, StringComparer.OrdinalIgnoreCase);

        return storedValue.Equals(filterValue, StringComparison.OrdinalIgnoreCase);
    }

    private static AttributeDataType ParseDataType(string dataType)
    {
        if (Enum.TryParse<AttributeDataType>(dataType, ignoreCase: true, out var result))
            return result;

        throw new ArgumentException(
            $"Invalid DataType: '{dataType}'. Valid values: String, Number, Select, MultiSelect, Boolean");
    }
}
