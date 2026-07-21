using BasketService.Application.Abstractions;
using BasketService.Application.Carts.Dtos;
using BasketService.Application.Errors;
using BasketService.Domain.Results;
using Mediator;

namespace BasketService.Application.Carts.Commands.AddCartItem;

public sealed class AddCartItemCommandHandler : ICommandHandler<AddCartItemCommand, Result<CartResponse>>
{
    private readonly ICartRepository _cartRepository;
    private readonly IAuthenticationContext _authenticationContext;

    public AddCartItemCommandHandler(ICartRepository cartRepository, IAuthenticationContext authenticationContext)
    {
        _cartRepository = cartRepository;
        _authenticationContext = authenticationContext;
    }

    public async ValueTask<Result<CartResponse>> Handle(AddCartItemCommand command, CancellationToken cancellationToken)
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

        cart.AddItem(command.ProductId, command.ProductName, command.UnitPrice, command.Quantity, command.ImageUrl);
        await _cartRepository.UpdateAsync(cart, cancellationToken);
        await _cartRepository.SaveChangesAsync(cancellationToken);

        return new CartResponse(cart);
    }
}