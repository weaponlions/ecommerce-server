using eShopServer.DTOs;
using eShopServer.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace eShopServer.Controllers;

/// <summary>
/// Public product endpoints for customers.
/// Supports browsing, searching, filtering by category and attributes.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    // ── Categories ──

    /// <summary>
    /// Returns all active product categories.
    /// </summary>
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
        => Ok(await _productService.GetActiveCategoriesAsync());

    /// <summary>
    /// Returns a single category with its attribute definitions.
    /// </summary>
    [HttpGet("categories/{slug}")]
    public async Task<IActionResult> GetCategoryBySlug(string slug)
    {
        var result = await _productService.GetCategoryBySlugAsync(slug);
        return result is null ? NotFound() : Ok(result);
    }

    // ── Products ──

    /// <summary>
    /// Returns a paginated, filterable list of products.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetProducts(
        [FromQuery] int? categoryId,
        [FromQuery] string? categorySlug,
        [FromQuery] int? collectionId,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] string? search,
        [FromQuery] string? sortBy,
        [FromQuery] bool sortDescending = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        // Parse dynamic attribute filters from query: attr_color=Red&attr_size=M
        var attributes = new Dictionary<string, string>();
        foreach (var key in Request.Query.Keys)
        {
            if (key.StartsWith("attr_", StringComparison.OrdinalIgnoreCase))
            {
                var attrName = key[5..]; // strip "attr_"
                attributes[attrName] = Request.Query[key].ToString();
            }
        }

        var filter = new ProductFilterRequest(
            categoryId, categorySlug, collectionId, minPrice, maxPrice,
            search, attributes.Count > 0 ? attributes : null,
            sortBy, sortDescending, page, pageSize
        );

        var result = await _productService.GetProductsAsync(filter);
        return Ok(result);
    }

    /// <summary>
    /// Returns a single product with its category attributes and variant siblings.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetProduct(int id)
    {
        var result = await _productService.GetProductByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Returns all active collections.
    /// </summary>
    [HttpGet("collections")]
    public async Task<IActionResult> GetCollections()
        => Ok(await _productService.GetActiveCollectionsAsync());

    /// <summary>
    /// Returns a single collection by ID.
    /// </summary>
    [HttpGet("collections/{id:int}")]
    public async Task<IActionResult> GetCollection(int id)
    {
        var result = await _productService.GetCollectionByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Returns all products belonging to a specific collection.
    /// </summary>
    [HttpGet("collections/{collectionId:int}/products")]
    public async Task<IActionResult> GetCollectionProducts(int collectionId)
    {
        var result = await _productService.GetCollectionProductsAsync(collectionId);
        return Ok(result);
    }

    /// <summary>
    /// Returns variant siblings for a given product (other items in the same VariantGroupId).
    /// </summary>
    [HttpGet("{id:int}/variants")]
    public async Task<IActionResult> GetVariants(int id)
    {
        var result = await _productService.GetVariantSiblingsAsync(id);
        return Ok(result);
    }
}
