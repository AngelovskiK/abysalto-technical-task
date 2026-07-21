using BasketService.Application.Carts.Commands.AddCartItem;
using BasketService.Application.Carts.Commands.ClearCart;
using BasketService.Application.Carts.Commands.RemoveCartItem;
using BasketService.Application.Carts.Commands.UpdateCartItemQuantity;
using BasketService.Application.Carts.Queries.GetCart;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace BasketService.Api.Controllers;

[Route("api/[controller]")]
public class CartController : BaseApiController
{
    private readonly IMediator _mediator;

    public CartController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCartQuery(), cancellationToken);
        return HandleResult(result, Ok);
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem([FromBody] AddCartItemCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result, Ok);
    }

    [HttpPut("items/{cartItemId:guid}")]
    public async Task<IActionResult> UpdateItem(Guid cartItemId, [FromBody] UpdateCartItemQuantityCommand command, CancellationToken cancellationToken)
    {
        command.CartItemId = cartItemId;
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result, Ok);
    }

    [HttpDelete("items/{cartItemId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid cartItemId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RemoveCartItemCommand { CartItemId = cartItemId }, cancellationToken);
        return HandleResult(result, Ok);
    }

    [HttpDelete]
    public async Task<IActionResult> Clear(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ClearCartCommand(), cancellationToken);
        return HandleResult(result, NoContent);
    }
}