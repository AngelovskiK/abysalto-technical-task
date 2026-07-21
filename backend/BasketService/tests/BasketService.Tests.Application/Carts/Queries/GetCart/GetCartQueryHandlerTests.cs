using BasketService.Application.Abstractions;
using BasketService.Application.Carts.Dtos;
using BasketService.Application.Carts.Queries.GetCart;
using BasketService.Domain.Entities;
using BasketService.Domain.Results;
using Moq;

namespace BasketService.Tests.Application.Carts.Queries;

public class GetCartQueryHandlerTests
{
    private readonly Mock<ICartRepository> _cartRepository = new();
    private readonly Mock<ICartCache> _cartCache = new();
    private readonly Mock<IAuthenticationContext> _authenticationContext = new();

    [Fact]
    public async Task Handle_WhenCartExists_ReturnsMappedCart()
    {
        var userId = Guid.NewGuid();
        var cart = Cart.Create(userId);
        cart.AddItem(Guid.NewGuid(), "Keyboard", 89.99m, 1);

        _authenticationContext.SetupGet(context => context.UserId).Returns(userId);
        _cartRepository
            .Setup(repository => repository.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        var handler = new GetCartQueryHandler(_cartRepository.Object, _cartCache.Object, _authenticationContext.Object);

        var result = await handler.Handle(new GetCartQuery(), CancellationToken.None);

        var success = Assert.IsType<Result<CartResponse>.Success>(result);
        Assert.Equal(cart.Id, success.Value.Id);
        Assert.Equal(userId, success.Value.UserId);
        Assert.Single(success.Value.Items);
        Assert.Equal(89.99m, success.Value.Total);
        _cartCache.Verify(cache => cache.SetAsync(userId, It.IsAny<CartResponse>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCartDoesNotExist_ReturnsNotFoundFailure()
    {
        var userId = Guid.NewGuid();

        _authenticationContext.SetupGet(context => context.UserId).Returns(userId);
        _cartRepository
            .Setup(repository => repository.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cart?)null);

        var handler = new GetCartQueryHandler(_cartRepository.Object, _cartCache.Object, _authenticationContext.Object);

        var result = await handler.Handle(new GetCartQuery(), CancellationToken.None);

        var failure = Assert.IsType<Result<CartResponse>.Failure>(result);
        Assert.Equal("NOT_FOUND", failure.Error.Code);
    }

    [Fact]
    public async Task Handle_WhenCartIsCached_ReturnsMappedCartWithoutQueryingRepository()
    {
        var userId = Guid.NewGuid();
        var cachedCart = new CartResponse(Guid.NewGuid(), userId, 42.5m, DateTime.UtcNow)
        {
            Items = []
        };

        _authenticationContext.SetupGet(context => context.UserId).Returns(userId);
        _cartCache
            .Setup(cache => cache.GetAsync<CartResponse>(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedCart);

        var handler = new GetCartQueryHandler(_cartRepository.Object, _cartCache.Object, _authenticationContext.Object);

        var result = await handler.Handle(new GetCartQuery(), CancellationToken.None);

        var success = Assert.IsType<Result<CartResponse>.Success>(result);
        Assert.Equal(cachedCart.Id, success.Value.Id);
        _cartRepository.Verify(repository => repository.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}