using BasketService.Domain.Results;
using Mediator;

namespace BasketService.Application.Users.Commands;

public class LoginCommand : ICommand<Result<LoginCommandResponse>>
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class LoginCommandResponse
{
    public Guid UserId { get; set; }
    public required string Email { get; set; }
    public required string Token { get; set; }
}
