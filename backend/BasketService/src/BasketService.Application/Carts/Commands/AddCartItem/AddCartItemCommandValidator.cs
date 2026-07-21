using FluentValidation;

namespace BasketService.Application.Carts.Commands.AddCartItem;

public sealed class AddCartItemCommandValidator : AbstractValidator<AddCartItemCommand>
{
    public AddCartItemCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.ProductName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.UnitPrice).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.ImageUrl).MaximumLength(2048).When(x => !string.IsNullOrWhiteSpace(x.ImageUrl));
    }
}