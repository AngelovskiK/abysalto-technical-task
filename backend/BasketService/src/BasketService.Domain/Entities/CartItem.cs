namespace BasketService.Domain.Entities;

/// <summary>
/// CartItem — a product in a user's cart.
/// Owned by Cart aggregate (no independent lifecycle).
/// </summary>
public class CartItem
{
    public Guid Id { get; set; }
    public Guid CartId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public string? ImageUrl { get; set; }

    // Navigation
    public Cart? Cart { get; set; }

    /// <summary>
    /// Factory method — creates a new CartItem.
    /// </summary>
    public static CartItem Create(Guid productId, string productName, decimal unitPrice, int quantity, string? imageUrl = null)
    {
        return new CartItem
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            ProductName = productName,
            UnitPrice = unitPrice,
            Quantity = quantity,
            ImageUrl = imageUrl
        };
    }
}
