using BankChu.CoreBanking.Application.Abstractions.Services;
using StackExchange.Redis;

namespace BankChu.CoreBanking.Infrastructure.Services;

public sealed class RedisIdempotencyService : IIdempotencyService
{
    private const string Prefix = "idempotency:";
    private readonly IDatabase _database;

    public RedisIdempotencyService(IConnectionMultiplexer multiplexer)
    {
        _database = multiplexer.GetDatabase();
    }

    public async Task<bool> TryAcquireAsync(
        string key,
        TimeSpan ttl,
        CancellationToken cancellationToken)
    {
        return await _database.StringSetAsync(
            BuildKey(key),
            "IN_PROGRESS",
            ttl,
            When.NotExists);
    }

    public async Task MarkCompletedAsync(
        string key,
        TimeSpan ttl,
        CancellationToken cancellationToken)
    {
        await _database.StringSetAsync(
            BuildKey(key),
            "COMPLETED",
            ttl);
    }

    public async Task<bool> ExistsAsync(
        string key,
        CancellationToken cancellationToken)
    {
        return await _database.KeyExistsAsync(BuildKey(key));
    }

    private static string BuildKey(string key)
        => $"{Prefix}{key}";
}
