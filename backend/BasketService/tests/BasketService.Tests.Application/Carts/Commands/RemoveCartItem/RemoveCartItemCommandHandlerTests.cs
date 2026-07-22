using BasketService.Application.Abstractions;
using BasketService.Application.Carts.Commands.RemoveCartItem;
using BasketService.Application.Carts.Dtos;
using BasketService.Domain.Entities;
using BasketService.Domain.Results;
using Moq;

namespace BasketService.Tests.Application.Carts.Commands.RemoveCartItem;

public class RemoveCartItemCommandHandlerTests
{
    private readonly Mock<ICartRepository> _cartRepository = new();
    private readonly Mock<ICartCache> _cartCache = new();
    private readonly Mock<IAuthenticationContext> _authenticationContext = new();

    [Fact]
    public async Task Handle_WhenCartItemExists_RemovesItemAndReturnsUpdatedCart()
    {
        var userId = Guid.NewGuid();
        var firstProductId = Guid.NewGuid();
        var secondProductId = Guid.NewGuid();
        var cart = Cart.Create(userId);
        cart.AddItem(firstProductId, "Mouse", 10m, 1);
        cart.AddItem(secondProductId, "Keyboard", 20m, 2);
        var cartItemId = cart.Items[0].Id;

        _authenticationContext.SetupGet(context => context.UserId).Returns(userId);
        _cartRepository
            .Setup(repository => repository.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        var handler = new RemoveCartItemCommandHandler(_cartRepository.Object, _cartCache.Object, _authenticationContext.Object);

        var result = await handler.Handle(new RemoveCartItemCommand
        {
            CartItemId = cartItemId
        }, CancellationToken.None);

        var success = Assert.IsType<Result<CartResponse>.Success>(result);
        Assert.Single(success.Value.Items);
        Assert.Equal(secondProductId, success.Value.Items[0].ProductId);
        Assert.Equal(40m, success.Value.Total);
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

        var handler = new RemoveCartItemCommandHandler(_cartRepository.Object, _cartCache.Object, _authenticationContext.Object);

        var result = await handler.Handle(new RemoveCartItemCommand
        {
            CartItemId = Guid.NewGuid()
        }, CancellationToken.None);

        var failure = Assert.IsType<Result<CartResponse>.Failure>(result);
        Assert.Equal("NOT_FOUND", failure.Error.Code);
        _cartRepository.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}