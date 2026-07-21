using System.IdentityModel.Tokens.Jwt;
using BasketService.Application.Abstractions;
using BasketService.Application.Users.Commands.Login;
using BasketService.Domain.Entities;
using BasketService.Domain.Results;
using Moq;

namespace BasketService.Tests.Application.Users.Commands;

public class LoginCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<ICartRepository> _cartRepository = new();

    [Fact]
    public async Task Handle_WhenUserExists_ReturnsExistingUserWithoutCreatingCart()
    {
        var existingUser = User.Create("jane@example.com", "Jane");
        var command = new LoginCommand
        {
            Email = existingUser.Email,
            Name = existingUser.Name
        };

        _userRepository
            .Setup(repository => repository.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        var handler = CreateHandler();

        var result = await handler.Handle(command, CancellationToken.None);

        var success = Assert.IsType<Result<LoginCommandResponse>.Success>(result);
        Assert.Equal(existingUser.Id, success.Value.UserId);
        Assert.Equal(existingUser.Email, success.Value.Email);
        Assert.False(string.IsNullOrWhiteSpace(success.Value.Token));

        var token = new JwtSecurityTokenHandler().ReadJwtToken(success.Value.Token);
        Assert.Equal(existingUser.Id.ToString(), token.Subject);
        Assert.Contains(token.Claims, claim => claim.Type == JwtRegisteredClaimNames.Email && claim.Value == existingUser.Email);

        _userRepository.Verify(repository => repository.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _cartRepository.Verify(repository => repository.AddAsync(It.IsAny<Cart>(), It.IsAny<CancellationToken>()), Times.Never);
        _userRepository.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _cartRepository.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_CreatesUserAndCart()
    {
        var command = new LoginCommand
        {
            Email = "new.user@example.com",
            Name = "New User"
        };

        _userRepository
            .Setup(repository => repository.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        User? createdUser = null;
        Cart? createdCart = null;

        _userRepository
            .Setup(repository => repository.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((user, _) => createdUser = user)
            .Returns(Task.CompletedTask);

        _cartRepository
            .Setup(repository => repository.AddAsync(It.IsAny<Cart>(), It.IsAny<CancellationToken>()))
            .Callback<Cart, CancellationToken>((cart, _) => createdCart = cart)
            .Returns(Task.CompletedTask);

        var handler = CreateHandler();

        var result = await handler.Handle(command, CancellationToken.None);

        var success = Assert.IsType<Result<LoginCommandResponse>.Success>(result);
        Assert.NotNull(createdUser);
        Assert.NotNull(createdCart);
        Assert.Equal(createdUser!.Id, createdCart!.UserId);
        Assert.Equal(createdUser.Id, success.Value.UserId);
        Assert.Equal(command.Email, success.Value.Email);

        _userRepository.Verify(repository => repository.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _cartRepository.Verify(repository => repository.AddAsync(It.IsAny<Cart>(), It.IsAny<CancellationToken>()), Times.Once);
        _userRepository.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cartRepository.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private LoginCommandHandler CreateHandler() =>
        new(_userRepository.Object, _cartRepository.Object);
}