namespace BasketService.Application.Abstractions;

/// <summary>
/// Provides the current authenticated user context extracted from the JWT bearer token.
/// Injected into all command/query handlers.
/// </summary>
public interface IAuthenticationContext
{
    /// <summary>
    /// The authenticated user ID from the JWT 'sub' claim.
    /// Null if the request is unauthenticated.
    /// </summary>
    Guid? UserId { get; }

    /// <summary>
    /// The email claim from the JWT. Null if unauthenticated.
    /// </summary>
    string? Email { get; }
}
