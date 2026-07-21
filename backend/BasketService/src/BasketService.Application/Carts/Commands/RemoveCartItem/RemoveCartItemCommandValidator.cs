using FluentValidation;

namespace BasketService.Application.Carts.Commands.RemoveCartItem;

public sealed class RemoveCartItemCommandValidator : AbstractValidator<RemoveCartItemCommand>
{
    public RemoveCartItemCommandValidator()
    {
        RuleFor(x => x.CartItemId).NotEmpty();
    }
}