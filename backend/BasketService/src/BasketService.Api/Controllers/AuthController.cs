using BasketService.Application.Users.Commands.Login;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BasketService.Api.Controllers;

/// <summary>
/// Authentication endpoints for user login and token generation.
/// </summary>
[Route("api/[controller]")]
public class AuthController : BaseApiController
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Login or create a user and receive a JWT token.
    /// </summary>
    /// <param name="command">Email and Name</param>
    /// <returns>User ID and JWT token on success</returns>
    [HttpPost("login")]
    [EnableRateLimiting("fixed")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result, Ok);
    }
}
