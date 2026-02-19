namespace eShopServer.Models;

/// <summary>
/// Join table linking Products to Collections (many-to-many).
/// A product can belong to multiple collections (e.g., "Summer Sale" AND "Menswear"),
/// and a collection can contain multiple products.
/// </summary>
public class ProductCollection
{
    public int ProductId { get; set; }
    public int CollectionId { get; set; }

    /// <summary>
    /// Controls the display order of this product within the collection.
    /// </summary>
    public int DisplayOrder { get; set; }

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    // ── Navigation ──
    public Product Product { get; set; } = null!;
    public Collection Collection { get; set; } = null!;
}
