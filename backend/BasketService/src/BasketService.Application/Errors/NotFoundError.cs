using BasketService.Domain.Errors;

namespace BasketService.Application.Errors;

public sealed record NotFoundError : ApplicationError
{
    public NotFoundError(string resourceName, string identifier)
    {
        Code = "NOT_FOUND";
        Message = $"{resourceName} '{identifier}' was not found.";
    }
}