using BasketService.Domain.Entities;

namespace BasketService.Application.Carts.Dtos;

public record CartResponse(Guid Id, Guid UserId, decimal Total, DateTime UpdatedAt)
{
    public IReadOnlyList<CartItemResponse> Items { get; set; } = [];

    public CartResponse(Cart cart) : this(cart.Id, cart.UserId, cart.GetTotal(), cart.UpdatedAt)
    {
        Items = [.. cart.Items
            .Select(item => new CartItemResponse
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity,
                ImageUrl = item.ImageUrl
            })];
    }
}
