using BasketService.Application.Abstractions;
using BasketService.Application.Carts.Commands.UpdateCartItemQuantity;
using BasketService.Application.Carts.Dtos;
using BasketService.Domain.Entities;
using BasketService.Domain.Results;
using Moq;

namespace BasketService.Tests.Application.Carts.Commands.UpdateCartItemQuantity;

public class UpdateCartItemQuantityCommandHandlerTests
{
    private readonly Mock<ICartRepository> _cartRepository = new();
    private readonly Mock<ICartCache> _cartCache = new();
    private readonly Mock<IAuthenticationContext> _authenticationContext = new();

    [Fact]
    public async Task Handle_WhenCartItemExists_UpdatesQuantityAndReturnsUpdatedCart()
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var cart = Cart.Create(userId);
        cart.AddItem(productId, "Keyboard", 50m, 1);
        var cartItemId = cart.Items[0].Id;

        _authenticationContext.SetupGet(context => context.UserId).Returns(userId);
        _cartRepository
            .Setup(repository => repository.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        var handler = new UpdateCartItemQuantityCommandHandler(_cartRepository.Object, _cartCache.Object, _authenticationContext.Object);

        var result = await handler.Handle(new UpdateCartItemQuantityCommand
        {
            CartItemId = cartItemId,
            Quantity = 3
        }, CancellationToken.None);

        var success = Assert.IsType<Result<CartResponse>.Success>(result);
        Assert.Equal(3, success.Value.Items[0].Quantity);
        Assert.Equal(150m, success.Value.Total);
        _cartRepository.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cartCache.Verify(cache => cache.RemoveAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCartItemDoesNotExist_ReturnsNotFoundFailure()
    {
        var userId = Guid.NewGuid();
        var cart = Cart.Create(userId);

        _authenticationContext.SetupGet(context => context.UserId).Returns(userId);
        _cartRepository
            .Setup(repository => repository.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        var handler = new UpdateCartItemQuantityCommandHandler(_cartRepository.Object, _cartCache.Object, _authenticationContext.Object);

        var result = await handler.Handle(new UpdateCartItemQuantityCommand
        {
            CartItemId = Guid.NewGuid(),
            Quantity = 2
        }, CancellationToken.None);

        var failure = Assert.IsType<Result<CartResponse>.Failure>(result);
        Assert.Equal("NOT_FOUND", failure.Error.Code);
        _cartRepository.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}