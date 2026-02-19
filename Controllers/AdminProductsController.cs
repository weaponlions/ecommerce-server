using eShopServer.DTOs;
using eShopServer.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace eShopServer.Controllers;

/// <summary>
/// Admin endpoints for managing categories, attributes, and products.
/// In production, protect with authentication/authorization.
/// </summary>
[ApiController]
[Route("api/admin/products")]
public class AdminProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public AdminProductsController(IProductService productService)
    {
        _productService = productService;
    }

    // ════════════════════════════════════════════════════════════════
    //  Categories
    // ════════════════════════════════════════════════════════════════

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
        => Ok(await _productService.GetActiveCategoriesAsync());

    [HttpGet("categories/{id:int}")]
    public async Task<IActionResult> GetCategory(int id)
    {
        var result = await _productService.GetCategoryByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory([FromBody] UpsertCategoryRequest request)
    {
        var result = await _productService.CreateCategoryAsync(request);
        return CreatedAtAction(nameof(GetCategory), new { id = result.Id }, result);
    }

    [HttpPut("categories/{id:int}")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpsertCategoryRequest request)
    {
        var result = await _productService.UpdateCategoryAsync(id, request);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("categories/{id:int}")]
    public async Task<IActionResult> DeleteCategory(int id)
        => await _productService.DeleteCategoryAsync(id) ? NoContent() : NotFound();

    // ════════════════════════════════════════════════════════════════
    //  Category Attributes
    // ════════════════════════════════════════════════════════════════

    [HttpGet("categories/{categoryId:int}/attributes")]
    public async Task<IActionResult> GetCategoryAttributes(int categoryId)
        => Ok(await _productService.GetCategoryAttributesAsync(categoryId));

    [HttpPost("categories/{categoryId:int}/attributes")]
    public async Task<IActionResult> CreateCategoryAttribute(
        int categoryId, [FromBody] UpsertCategoryAttributeRequest request)
    {
        try
        {
            var result = await _productService.CreateCategoryAttributeAsync(categoryId, request);
            return CreatedAtAction(nameof(GetCategoryAttributes), new { categoryId }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("attributes/{id:int}")]
    public async Task<IActionResult> UpdateCategoryAttribute(
        int id, [FromBody] UpsertCategoryAttributeRequest request)
    {
        try
        {
            var result = await _productService.UpdateCategoryAttributeAsync(id, request);
            return result is null ? NotFound() : Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("attributes/{id:int}")]
    public async Task<IActionResult> DeleteCategoryAttribute(int id)
        => await _productService.DeleteCategoryAttributeAsync(id) ? NoContent() : NotFound();

    // ════════════════════════════════════════════════════════════════
    //  Products
    // ════════════════════════════════════════════════════════════════

    [HttpGet]
    public async Task<IActionResult> GetProducts(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var filter = new ProductFilterRequest(
            null, null, null, null, null, null, null, null, false, page, pageSize);
        var result = await _productService.GetProductsAsync(filter);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetProduct(int id)
    {
        var result = await _productService.GetProductByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        try
        {
            var result = await _productService.CreateProductAsync(request);
            return CreatedAtAction(nameof(GetProduct), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductRequest request)
    {
        try
        {
            var result = await _productService.UpdateProductAsync(id, request);
            return result is null ? NotFound() : Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProduct(int id)
        => await _productService.DeleteProductAsync(id) ? NoContent() : NotFound();

    // ════════════════════════════════════════════════════════════════
    //  Collection ↔ Product Management
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Lists all products assigned to a collection.
    /// </summary>
    [HttpGet("~/api/admin/collections/{collectionId:int}/products")]
    public async Task<IActionResult> GetCollectionProducts(int collectionId)
    {
        var result = await _productService.GetCollectionProductsAsync(collectionId);
        return Ok(result);
    }

    /// <summary>
    /// Adds a product to a collection.
    /// </summary>
    [HttpPost("~/api/admin/collections/{collectionId:int}/products")]
    public async Task<IActionResult> AddProductToCollection(
        int collectionId, [FromBody] AddProductToCollectionRequest request)
    {
        try
        {
            await _productService.AddProductToCollectionAsync(collectionId, request);
            return Ok(new { message = "Product added to collection." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Removes a product from a collection.
    /// </summary>
    [HttpDelete("~/api/admin/collections/{collectionId:int}/products/{productId:int}")]
    public async Task<IActionResult> RemoveProductFromCollection(int collectionId, int productId)
        => await _productService.RemoveProductFromCollectionAsync(collectionId, productId)
            ? NoContent()
            : NotFound();
}
