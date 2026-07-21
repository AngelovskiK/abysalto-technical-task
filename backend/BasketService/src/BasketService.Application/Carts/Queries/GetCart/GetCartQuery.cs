using BasketService.Application.Carts.Dtos;
using BasketService.Domain.Results;
using Mediator;

namespace BasketService.Application.Carts.Queries.GetCart;

public sealed class GetCartQuery : IQuery<Result<CartResponse>>
{
}
