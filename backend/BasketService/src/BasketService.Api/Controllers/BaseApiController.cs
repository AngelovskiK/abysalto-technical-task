using BasketService.Application.Errors;
using BasketService.Domain.Errors;
using BasketService.Domain.Results;
using Microsoft.AspNetCore.Mvc;

namespace BasketService.Api.Controllers;

/// <summary>
/// Base controller with helper methods for handling Result types.
/// </summary>
[ApiController]
public abstract class BaseApiController : ControllerBase
{
    /// <summary>
    /// Handle a Result&lt;T&gt; by calling the onSuccess function if successful,
    /// or returning a ProblemDetails if failed.
    /// </summary>
    protected IActionResult HandleResult<T>(
        Result<T> result,
        Func<T, IActionResult> onSuccess)
    {
        return result switch
        {
            Result<T>.Success success => onSuccess(success.Value),
            Result<T>.Failure failure => Problem(
                detail: failure.Error.Message,
                statusCode: GetStatusCode(failure.Error),
                title: failure.Error.Code),
            _ => Problem("An unexpected error occurred", statusCode: StatusCodes.Status500InternalServerError)
        };
    }

    /// <summary>
    /// Handle a Result by calling the onSuccess function if successful,
    /// or returning a ProblemDetails if failed.
    /// </summary>
    protected IActionResult HandleResult(
        Result result,
        Func<IActionResult> onSuccess)
    {
        return result switch
        {
            Result.Success => onSuccess(),
            Result.Failure failure => Problem(
                detail: failure.Error.Message,
                statusCode: GetStatusCode(failure.Error),
                title: failure.Error.Code),
            _ => Problem("An unexpected error occurred", statusCode: StatusCodes.Status500InternalServerError)
        };
    }

    /// <summary>
    /// Map error types to appropriate HTTP status codes.
    /// </summary>
    private static int GetStatusCode(ApplicationError error) =>
        error switch
        {
            ValidationError => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status400BadRequest
        };
}
