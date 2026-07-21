using BasketService.Application.Abstractions;
using BasketService.Application.Carts.Commands.AddCartItem;
using BasketService.Application.Carts.Dtos;
using BasketService.Domain.Entities;
using BasketService.Domain.Results;
using Moq;

namespace BasketService.Tests.Application.Carts.Commands;

public class AddCartItemCommandHandlerTests
{
    private readonly Mock<ICartRepository> _cartRepository = new();
    private readonly Mock<ICartCache> _cartCache = new();
    private readonly Mock<IAuthenticationContext> _authenticationContext = new();

    [Fact]
    public async Task Handle_WhenUserIsUnauthenticated_ReturnsUnauthorizedFailure()
    {
        _authenticationContext.SetupGet(context => context.UserId).Returns((Guid?)null);

        var handler = CreateHandler();

        var result = await handler.Handle(new AddCartItemCommand(), CancellationToken.None);

        var failure = Assert.IsType<Result<CartResponse>.Failure>(result);
        Assert.Equal("UNAUTHORIZED", failure.Error.Code);
        _cartRepository.Verify(repository => repository.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCartExists_AddsItemAndReturnsUpdatedCart()
    {
        var userId = Guid.NewGuid();
        var cart = Cart.Create(userId);
        var command = new AddCartItemCommand
        {
            ProductId = Guid.NewGuid(),
            ProductName = "Wireless Mouse",
            UnitPrice = 25.50m,
            Quantity = 2,
            ImageUrl = "https://example.test/mouse.png"
        };

        _authenticationContext.SetupGet(context => context.UserId).Returns(userId);
        _cartRepository
            .Setup(repository => repository.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        var handler = CreateHandler();

        var result = await handler.Handle(command, CancellationToken.None);

        var success = Assert.IsType<Result<CartResponse>.Success>(result);
        Assert.Single(success.Value.Items);
        Assert.Equal(command.ProductId, success.Value.Items[0].ProductId);
        Assert.Equal(command.Quantity, success.Value.Items[0].Quantity);
        Assert.Equal(51.00m, success.Value.Total);

        _cartRepository.Verify(repository => repository.UpdateAsync(cart, It.IsAny<CancellationToken>()), Times.Once);
        _cartRepository.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cartCache.Verify(cache => cache.RemoveAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    private AddCartItemCommandHandler CreateHandler() =>
        new(_cartRepository.Object, _cartCache.Object, _authenticationContext.Object);
}