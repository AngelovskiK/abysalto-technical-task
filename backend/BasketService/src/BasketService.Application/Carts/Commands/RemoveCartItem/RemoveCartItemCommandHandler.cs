using BasketService.Application.Abstractions;
using BasketService.Application.Carts.Dtos;
using BasketService.Application.Errors;
using BasketService.Domain.Results;
using Mediator;

namespace BasketService.Application.Carts.Commands.RemoveCartItem;

public sealed class RemoveCartItemCommandHandler : ICommandHandler<RemoveCartItemCommand, Result<CartResponse>>
{
    private readonly ICartRepository _cartRepository;
    private readonly ICartCache _cartCache;
    private readonly IAuthenticationContext _authenticationContext;

    public RemoveCartItemCommandHandler(ICartRepository cartRepository, ICartCache cartCache, IAuthenticationContext authenticationContext)
    {
        _cartRepository = cartRepository;
        _cartCache = cartCache;
        _authenticationContext = authenticationContext;
    }

    public async ValueTask<Result<CartResponse>> Handle(RemoveCartItemCommand command, CancellationToken cancellationToken)
    {
        if (_authenticationContext.UserId is not Guid userId)
        {
            return Result<CartResponse>.Fail(new UnauthorizedError());
        }

        var cart = await _cartRepository.GetByUserIdAsync(userId, cancellationToken);
        if (cart is null)
        {
            return Result<CartResponse>.Fail(new NotFoundError("Cart", userId.ToString()));
        }

        var item = cart.Items.FirstOrDefault(x => x.Id == command.CartItemId);
        if (item is null)
        {
            return Result<CartResponse>.Fail(new NotFoundError("Cart item", command.CartItemId.ToString()));
        }

        cart.RemoveItem(command.CartItemId);
        await _cartRepository.SaveChangesAsync(cancellationToken);
        await _cartCache.RemoveAsync(userId, cancellationToken);

        return new CartResponse(cart);
    }
}