using BasketService.Domain.Results;
using Mediator;

namespace BasketService.Application.Users.Commands.Login;

public class LoginCommand : ICommand<Result<LoginCommandResponse>>
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
