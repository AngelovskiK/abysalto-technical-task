using BasketService.Application.Abstractions;
using BasketService.Application.Carts.Commands.ClearCart;
using BasketService.Domain.Entities;
using BasketService.Domain.Results;
using Moq;

namespace BasketService.Tests.Application.Carts.Commands.ClearCart;

public class ClearCartCommandHandlerTests
{
    private readonly Mock<ICartRepository> _cartRepository = new();
    private readonly Mock<IAuthenticationContext> _authenticationContext = new();

    [Fact]
    public async Task Handle_WhenCartExists_ClearsCartAndReturnsSuccess()
    {
        var userId = Guid.NewGuid();
        var cart = Cart.Create(userId);
        cart.AddItem(Guid.NewGuid(), "Laptop", 1000m, 1);

        _authenticationContext.SetupGet(context => context.UserId).Returns(userId);
        _cartRepository
            .Setup(repository => repository.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        var handler = new ClearCartCommandHandler(_cartRepository.Object, _authenticationContext.Object);

        var result = await handler.Handle(new ClearCartCommand(), CancellationToken.None);

        Assert.IsType<Result.Success>(result);
        Assert.Empty(cart.Items);
        _cartRepository.Verify(repository => repository.UpdateAsync(cart, It.IsAny<CancellationToken>()), Times.Once);
        _cartRepository.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsUnauthenticated_ReturnsUnauthorizedFailure()
    {
        _authenticationContext.SetupGet(context => context.UserId).Returns((Guid?)null);

        var handler = new ClearCartCommandHandler(_cartRepository.Object, _authenticationContext.Object);

        var result = await handler.Handle(new ClearCartCommand(), CancellationToken.None);

        var failure = Assert.IsType<Result.Failure>(result);
        Assert.Equal("UNAUTHORIZED", failure.Error.Code);
        _cartRepository.Verify(repository => repository.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}