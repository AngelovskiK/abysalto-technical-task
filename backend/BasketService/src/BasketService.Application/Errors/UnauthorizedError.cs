using BasketService.Domain.Errors;

namespace BasketService.Application.Errors;

public sealed record UnauthorizedError : ApplicationError
{
    public UnauthorizedError(string message = "Authentication is required to access this resource.")
    {
        Code = "UNAUTHORIZED";
        Message = message;
    }
}