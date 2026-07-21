using BasketService.Application.Abstractions;
using BasketService.Application.Carts.Dtos;
using BasketService.Application.Errors;
using BasketService.Domain.Results;
using Mediator;

namespace BasketService.Application.Carts.Queries.GetCart;

public sealed class GetCartQueryHandler : IQueryHandler<GetCartQuery, Result<CartResponse>>
{
    private readonly ICartRepository _cartRepository;
    private readonly ICartCache _cartCache;
    private readonly IAuthenticationContext _authenticationContext;

    public GetCartQueryHandler(ICartRepository cartRepository, ICartCache cartCache, IAuthenticationContext authenticationContext)
    {
        _cartRepository = cartRepository;
        _cartCache = cartCache;
        _authenticationContext = authenticationContext;
    }

    public async ValueTask<Result<CartResponse>> Handle(GetCartQuery query, CancellationToken cancellationToken)
    {
        if (_authenticationContext.UserId is not Guid userId)
        {
            return Result<CartResponse>.Fail(new UnauthorizedError());
        }

        var cachedCart = await _cartCache.GetAsync<CartResponse>(userId, cancellationToken);
        if (cachedCart is not null)
        {
            return cachedCart;
        }

        var cart = await _cartRepository.GetByUserIdAsync(userId, cancellationToken);
        if (cart is null)
        {
            return Result<CartResponse>.Fail(new NotFoundError("Cart", userId.ToString()));
        }

        var response = new CartResponse(cart);
        await _cartCache.SetAsync(userId, response, cancellationToken);
        return response;
    }
}