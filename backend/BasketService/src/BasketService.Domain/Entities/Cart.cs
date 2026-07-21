namespace BasketService.Domain.Entities;

/// <summary>
/// Cart aggregate root — represents a user's shopping cart.
/// Tied to a User by UserId (stable identity).
/// </summary>
public class Cart
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public List<CartItem> Items { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Factory method — creates a new Cart for a user.
    /// </summary>
    public static Cart Create(Guid userId)
    {
        return new Cart
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Adds or updates an item in the cart.
    /// </summary>
    public void AddItem(Guid productId, string productName, decimal unitPrice, int quantity, string? imageUrl = null)
    {
        var existingItem = Items.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
        }
        else
        {
            Items.Add(CartItem.Create(productId, productName, unitPrice, quantity, imageUrl));
        }
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the quantity of an item, or removes it if quantity ≤ 0.
    /// </summary>
    public void UpdateItemQuantity(Guid cartItemId, int quantity)
    {
        var item = Items.FirstOrDefault(i => i.Id == cartItemId);
        if (item == null)
            return;

        if (quantity <= 0)
        {
            Items.Remove(item);
        }
        else
        {
            item.Quantity = quantity;
        }
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Removes an item by its ID.
    /// </summary>
    public void RemoveItem(Guid cartItemId)
    {
        var item = Items.FirstOrDefault(i => i.Id == cartItemId);
        if (item != null)
        {
            Items.Remove(item);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Clears all items from the cart.
    /// </summary>
    public void Clear()
    {
        Items.Clear();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Calculates the total price of all items.
    /// </summary>
    public decimal GetTotal()
    {
        return Items.Sum(i => i.UnitPrice * i.Quantity);
    }
}
