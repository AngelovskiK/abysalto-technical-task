using BasketService.Application.Users.Commands;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace BasketService.Api.Controllers;

/// <summary>
/// Authentication endpoints for user login and token generation.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginCommandResponse>> Login([FromBody] LoginCommand command)
    {
        var response = await _mediator.Send(command);
        return Ok(response);
    }
}
