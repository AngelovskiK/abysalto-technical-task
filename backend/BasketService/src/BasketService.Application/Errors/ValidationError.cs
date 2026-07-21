using BasketService.Domain.Errors;
using FluentValidation.Results;

namespace BasketService.Application.Errors;

/// <summary>
/// Represents validation errors from FluentValidation.
/// </summary>
public sealed record ValidationError : ApplicationError
{
    public IReadOnlyList<ValidationFailure> Failures { get; }

    public ValidationError(List<ValidationFailure> failures)
    {
        Code = "VALIDATION_ERROR";
        Message = string.Join("; ", failures.Select(f => $"{f.PropertyName}: {f.ErrorMessage}"));
        Failures = failures.AsReadOnly();
    }
}
