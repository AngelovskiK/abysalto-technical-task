using BasketService.Application.Carts.Commands.AddCartItem;

namespace BasketService.Tests.Application.Carts.Commands;

public class AddCartItemCommandValidatorTests
{
    private readonly AddCartItemCommandValidator _validator = new();

    [Fact]
    public async Task ValidateAsync_WhenCommandIsValid_ReturnsNoErrors()
    {
        var command = new AddCartItemCommand
        {
            ProductId = Guid.NewGuid(),
            ProductName = "Monitor",
            UnitPrice = 199.99m,
            Quantity = 1,
            ImageUrl = "https://example.test/monitor.png"
        };

        var result = await _validator.ValidateAsync(command);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_WhenRequiredFieldsAreMissing_ReturnsErrors()
    {
        var command = new AddCartItemCommand();

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(AddCartItemCommand.ProductId));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(AddCartItemCommand.ProductName));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(AddCartItemCommand.UnitPrice));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(AddCartItemCommand.Quantity));
    }
}