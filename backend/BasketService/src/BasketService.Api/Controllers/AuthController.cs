using BasketService.Application.Users.Commands;
using Mediator;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var result = await _mediator.Send(command);
        return HandleResult(result, Ok);
    }
}
