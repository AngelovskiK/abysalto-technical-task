using FluentValidation;

namespace BasketService.Application.Carts.Commands.UpdateCartItemQuantity;

public sealed class UpdateCartItemQuantityCommandValidator : AbstractValidator<UpdateCartItemQuantityCommand>
{
    public UpdateCartItemQuantityCommandValidator()
    {
        RuleFor(x => x.CartItemId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}