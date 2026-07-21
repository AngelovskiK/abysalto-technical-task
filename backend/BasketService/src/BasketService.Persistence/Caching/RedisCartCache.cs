using System.Text.Json;
using BasketService.Application.Abstractions;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace BasketService.Persistence.Caching;

public sealed class RedisCartCache : ICartCache
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly ILogger<RedisCartCache> _logger;

    public RedisCartCache(IConnectionMultiplexer connectionMultiplexer, ILogger<RedisCartCache> logger)
    {
        _connectionMultiplexer = connectionMultiplexer;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(Guid userId, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var database = _connectionMultiplexer.GetDatabase();
            var value = await database.StringGetAsync(GetKey(userId));
            if (value.IsNullOrEmpty)
            {
                return null;
            }

            return JsonSerializer.Deserialize<T>(value.ToString(), SerializerOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read cart {UserId} from Redis cache", userId);
            return null;
        }
    }

    public async Task SetAsync<T>(Guid userId, T value, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var database = _connectionMultiplexer.GetDatabase();
            var payload = JsonSerializer.Serialize(value, SerializerOptions);
            await database.StringSetAsync(GetKey(userId), payload, CacheDuration);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to write cart {UserId} to Redis cache", userId);
        }
    }

    public async Task RemoveAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var database = _connectionMultiplexer.GetDatabase();
            await database.KeyDeleteAsync(GetKey(userId));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to remove cart {UserId} from Redis cache", userId);
        }
    }

    private static string GetKey(Guid userId) => $"cart:{userId}";
}