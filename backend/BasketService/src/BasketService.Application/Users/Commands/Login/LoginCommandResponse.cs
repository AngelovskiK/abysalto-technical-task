namespace BasketService.Application.Users.Commands.Login;

public class LoginCommandResponse
{
    public Guid UserId { get; set; }
    public required string Email { get; set; }
    public required string Token { get; set; }
}
