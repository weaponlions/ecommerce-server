using System.ComponentModel.DataAnnotations;

namespace eShopServer.Models;

public class RecentlyVisitedProduct
{
    /// <summary>
    /// EF Core requires a parameterless constructor.
    /// </summary>
    private RecentlyVisitedProduct() { }

    public RecentlyVisitedProduct(string userId, int productId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId is required.", nameof(userId));
        if (productId <= 0)
            throw new ArgumentOutOfRangeException(nameof(productId), "ProductId must be a positive number.");

        UserId = userId.Trim();
        ProductId = productId;
    }

    public int Id { get; set; }

    /// <summary>
    /// The user who visited the product. 
    /// Can be a user ID or anonymous session ID.
    /// </summary>
    [Required(ErrorMessage = "UserId is required.")]
    [MaxLength(100, ErrorMessage = "UserId cannot exceed 100 characters.")]
    public string UserId { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "ProductId must be a positive number.")]
    public int ProductId { get; set; }

    public Product Product { get; set; } = null!;

    public DateTime VisitedAt { get; set; } = DateTime.UtcNow;
}
