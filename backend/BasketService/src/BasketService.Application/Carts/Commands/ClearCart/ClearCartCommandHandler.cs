using BasketService.Application.Abstractions;
using BasketService.Application.Errors;
using BasketService.Domain.Results;
using Mediator;

namespace BasketService.Application.Carts.Commands.ClearCart;

public sealed class ClearCartCommandHandler : ICommandHandler<ClearCartCommand, Result>
{
    private readonly ICartRepository _cartRepository;
    private readonly ICartCache _cartCache;
    private readonly IAuthenticationContext _authenticationContext;

    public ClearCartCommandHandler(ICartRepository cartRepository, ICartCache cartCache, IAuthenticationContext authenticationContext)
    {
        _cartRepository = cartRepository;
        _cartCache = cartCache;
        _authenticationContext = authenticationContext;
    }

    public async ValueTask<Result> Handle(ClearCartCommand command, CancellationToken cancellationToken)
    {
        if (_authenticationContext.UserId is not Guid userId)
        {
            return Result.Fail(new UnauthorizedError());
        }

        var cart = await _cartRepository.GetByUserIdAsync(userId, cancellationToken);
        if (cart is null)
        {
            return Result.Fail(new NotFoundError("Cart", userId.ToString()));
        }

        cart.Clear();
        await _cartRepository.UpdateAsync(cart, cancellationToken);
        await _cartRepository.SaveChangesAsync(cancellationToken);
        await _cartCache.RemoveAsync(userId, cancellationToken);

        return Result.Ok();
    }
}