using BankChu.CoreBanking.Application.Abstractions.Services;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace BankChu.CoreBanking.Infrastructure.Services;

public sealed class RedisIdempotencyService : IIdempotencyService
{
    private const string Prefix = "idempotency:";
    private readonly IDatabase _database;
    private readonly ILogger<RedisIdempotencyService> _logger;

    public RedisIdempotencyService(
        IConnectionMultiplexer multiplexer,
        ILogger<RedisIdempotencyService> logger)
    {
        _database = multiplexer.GetDatabase();
        _logger = logger;
    }

    public async Task<bool> TryAcquireAsync(
        string key,
        TimeSpan ttl,
        CancellationToken cancellationToken)
    {
        var redisKey = BuildKey(key);

        _logger.LogDebug("Attempting to acquire idempotency lock. KeyHash={KeyHash}", redisKey.GetHashCode());

        try
        {
            var acquired = await _database.StringSetAsync(
                redisKey,
                "IN_PROGRESS",
                ttl,
                When.NotExists);

            if (!acquired)
            {
                _logger.LogWarning("Idempotency lock already exists. KeyHash={KeyHash}", redisKey.GetHashCode());
            }

            return acquired;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error while acquiring idempotency lock. KeyHash={KeyHash}",
                redisKey.GetHashCode());

            throw;
        }
    }

    public async Task MarkCompletedAsync(
        string key,
        TimeSpan ttl,
        CancellationToken cancellationToken)
    {
        var redisKey = BuildKey(key);

        try
        {
            await _database.StringSetAsync(redisKey, "COMPLETED", ttl);

            _logger.LogDebug("Idempotency lock marked as completed. KeyHash={KeyHash}", redisKey.GetHashCode());
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error while marking idempotency lock as completed. KeyHash={KeyHash}",
                redisKey.GetHashCode());

            throw;
        }
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken)
    {
        return await _database.KeyExistsAsync(BuildKey(key));
    }

    private static string BuildKey(string key)
        => $"{Prefix}{key}";
}
