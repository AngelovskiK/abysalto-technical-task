namespace BasketService.Domain.Errors;

/// <summary>
/// Base class for application errors.
/// </summary>
public abstract record ApplicationError
{
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
