namespace BasketService.Domain.Results;

/// <summary>
/// Represents the outcome of an operation with no return value.
/// Use Result.Success() for success, Result.Failure(error) for failure.
/// </summary>
public abstract record Result
{
    public sealed record Success : Result;
    public sealed record Failure(string Code, string Message) : Result;

    public static Result Ok() => new Success();
    public static Result Fail(string code, string message) => new Failure(code, message);
}

/// <summary>
/// Represents the outcome of an operation with a return value of type T.
/// Use Result.Success(value) for success, Result.Failure(error) for failure.
/// </summary>
public abstract record Result<T>
{
    public sealed record Success(T Value) : Result<T>;
    public sealed record Failure(string Code, string Message) : Result<T>;

    public static Result<T> Ok(T value) => new Success(value);
    public static Result<T> Fail(string code, string message) => new Failure(code, message);

    /// <summary>
    /// Execute a function based on success/failure state.
    /// </summary>
    public TResult Match<TResult>(
        Func<T, TResult> onSuccess,
        Func<string, string, TResult> onFailure) =>
        this switch
        {
            Success s => onSuccess(s.Value),
            Failure f => onFailure(f.Code, f.Message),
            _ => throw new InvalidOperationException("Unknown result type")
        };

    /// <summary>
    /// Execute an action based on success/failure state.
    /// </summary>
    public void Match(
        Action<T> onSuccess,
        Action<string, string> onFailure)
    {
        switch (this)
        {
            case Success s:
                onSuccess(s.Value);
                break;
            case Failure f:
                onFailure(f.Code, f.Message);
                break;
        }
    }
}
