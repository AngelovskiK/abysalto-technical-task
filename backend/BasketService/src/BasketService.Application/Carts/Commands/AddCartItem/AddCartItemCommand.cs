using BasketService.Application.Carts.Dtos;
using BasketService.Domain.Results;
using Mediator;

namespace BasketService.Application.Carts.Commands.AddCartItem;

public sealed class AddCartItemCommand : ICommand<Result<CartResponse>>
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public string? ImageUrl { get; set; }
}