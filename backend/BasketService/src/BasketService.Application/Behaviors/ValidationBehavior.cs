using BasketService.Application.Errors;
using BasketService.Domain.Errors;
using BasketService.Domain.Results;
using FluentValidation;
using Mediator;

namespace BasketService.Application.Behaviors;

/// <summary>
/// Mediator pipeline behavior that validates requests before they reach handlers.
/// Automatically returns Result.Failure with ValidationError if validation fails.
/// </summary>
public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IMessage
    where TResponse : class
{
    public async ValueTask<TResponse> Handle(
        TRequest message,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        if (!validators.Any())
        {
            return await next(message, cancellationToken);
        }

        var context = new ValidationContext<TRequest>(message);
        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .Where(r => r.Errors.Count > 0)
            .SelectMany(r => r.Errors)
            .ToList();

        if (failures.Count == 0)
        {
            return await next(message, cancellationToken);
        }

        var error = new ValidationError(failures);

        // Handle Result<T>
        if (typeof(TResponse).IsGenericType &&
            typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            var innerType = typeof(TResponse).GetGenericArguments()[0];
            var failureMethod = typeof(Result<>)
                .MakeGenericType(innerType)
                .GetMethod(nameof(Result<object>.Fail), [typeof(ApplicationError)]);

            return (TResponse)failureMethod!.Invoke(null, [error])!;
        }

        // Handle Result (non-generic)
        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)(object)Result.Fail(error);
        }

        // If it's not a Result type, throw exception
        throw new ValidationException(string.Join("; ", failures.Select(f => f.ErrorMessage)));
    }
}
