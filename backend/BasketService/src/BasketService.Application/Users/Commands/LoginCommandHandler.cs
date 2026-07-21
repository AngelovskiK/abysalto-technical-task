using BasketService.Application.Abstractions;
using BasketService.Domain.Entities;
using BasketService.Domain.Results;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BasketService.Application.Users.Commands;

/// <summary>
/// Handles login by creating or retrieving a User and issuing a JWT token.
/// For local dev: any email is accepted.
/// Creates a Cart for new users.
/// </summary>
public class LoginCommandHandler : ICommandHandler<LoginCommand, Result<LoginCommandResponse>>
{
    private const string SecretKey = "dev-secret-key-change-me-in-production";
    private readonly IUserRepository _userRepository;
    private readonly ICartRepository _cartRepository;

    public LoginCommandHandler(IUserRepository userRepository, ICartRepository cartRepository)
    {
        _userRepository = userRepository;
        _cartRepository = cartRepository;
    }

    public async ValueTask<Result<LoginCommandResponse>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        // Check if user already exists
        var user = await _userRepository.GetByEmailAsync(command.Email, cancellationToken);

        if (user == null)
        {
            // Create new user and cart
            user = User.Create(command.Email, command.Name);
            await _userRepository.AddAsync(user, cancellationToken);

            var cart = Cart.Create(user.Id);
            await _cartRepository.AddAsync(cart, cancellationToken);

            await _userRepository.SaveChangesAsync(cancellationToken);
            await _cartRepository.SaveChangesAsync(cancellationToken);
        }

        var token = GenerateJwtToken(user.Id, user.Email);

        var response = new LoginCommandResponse
        {
            UserId = user.Id,
            Email = user.Email,
            Token = token
        };

        return Result<LoginCommandResponse>.Ok(response);
    }

    private static string GenerateJwtToken(Guid userId, string email)
    {
        var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: "basket-api",
            audience: "basket-api-client",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
