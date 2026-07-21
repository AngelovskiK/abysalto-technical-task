using BasketService.Application.Carts.Dtos;
using BasketService.Domain.Results;
using Mediator;

namespace BasketService.Application.Carts.Commands.RemoveCartItem;

public sealed class RemoveCartItemCommand : ICommand<Result<CartResponse>>
{
    public Guid CartItemId { get; set; }
}