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

        var handler = new GetCartQueryHandler(_cartRepository.Object, _authenticationContext.Object);

        var result = await handler.Handle(new GetCartQuery(), CancellationToken.None);

        var success = Assert.IsType<Result<CartResponse>.Success>(result);
        Assert.Equal(cart.Id, success.Value.Id);
        Assert.Equal(userId, success.Value.UserId);
        Assert.Single(success.Value.Items);
        Assert.Equal(89.99m, success.Value.Total);
    }

    [Fact]
    public async Task Handle_WhenCartDoesNotExist_ReturnsNotFoundFailure()
    {
        var userId = Guid.NewGuid();

        _authenticationContext.SetupGet(context => context.UserId).Returns(userId);
        _cartRepository
            .Setup(repository => repository.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cart?)null);

        var handler = new GetCartQueryHandler(_cartRepository.Object, _authenticationContext.Object);

        var result = await handler.Handle(new GetCartQuery(), CancellationToken.None);

        var failure = Assert.IsType<Result<CartResponse>.Failure>(result);
        Assert.Equal("NOT_FOUND", failure.Error.Code);
    }
}