using Mediator;

namespace BasketService.Application.Users.Commands;

/// <summary>
/// Command to authenticate a user and issue a JWT token.
/// For local dev: accepts any email and returns a valid JWT.
/// </summary>
public class LoginCommand : ICommand<LoginCommandResponse>
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class LoginCommandResponse
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}
