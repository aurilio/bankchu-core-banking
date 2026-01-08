namespace BankChu.CoreBanking.Application.Abstractions.Services;

public interface IIdempotencyService
{
    Task<bool> TryAcquireAsync(string key, TimeSpan ttl, CancellationToken cancellationToken);

    Task MarkCompletedAsync(string key, TimeSpan ttl, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken);
}