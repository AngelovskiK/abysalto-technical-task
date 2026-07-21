using BasketService.Domain.Entities;

namespace BasketService.Application.Abstractions;

/// <summary>
/// Repository for User persistence.
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository for Cart persistence.
/// </summary>
public interface ICartRepository
{
    Task<Cart?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Cart?> GetByIdAsync(Guid cartId, CancellationToken cancellationToken = default);
    Task AddAsync(Cart cart, CancellationToken cancellationToken = default);
    Task UpdateAsync(Cart cart, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Cache abstraction for cart reads.
/// </summary>
public interface ICartCache
{
    Task<T?> GetAsync<T>(Guid userId, CancellationToken cancellationToken = default) where T : class;
    Task SetAsync<T>(Guid userId, T value, CancellationToken cancellationToken = default) where T : class;
    Task RemoveAsync(Guid userId, CancellationToken cancellationToken = default);
}
