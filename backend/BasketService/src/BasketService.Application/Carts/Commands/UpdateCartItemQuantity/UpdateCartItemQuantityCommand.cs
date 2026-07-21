using BasketService.Application.Carts.Dtos;
using BasketService.Domain.Results;
using Mediator;

namespace BasketService.Application.Carts.Commands.UpdateCartItemQuantity;

public sealed class UpdateCartItemQuantityCommand : ICommand<Result<CartResponse>>
{
    public Guid CartItemId { get; set; }
    public int Quantity { get; set; }
}