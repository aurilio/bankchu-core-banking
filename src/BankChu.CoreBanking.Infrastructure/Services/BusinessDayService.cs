using BankChu.CoreBanking.Application.Abstractions.Services;
using BankChu.CoreBanking.Infrastructure.External;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace BankChu.CoreBanking.Infrastructure.Services;

public sealed class BusinessDayService : IBusinessDayService
{
    private const string CacheKeyPrefix = "holidays";
    private static readonly TimeSpan CacheTimeToLive = TimeSpan.FromHours(24);

    private readonly IBrasilApiClient _brasilApiClient;
    private readonly IDistributedCache _cache;

    public BusinessDayService(
        IBrasilApiClient brasilApiClient,
        IDistributedCache cache)
    {
        _brasilApiClient = brasilApiClient;
        _cache = cache;
    }

    public async Task<bool> IsBusinessDayAsync(DateOnly date, CancellationToken cancellationToken)
    {
        if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            return false;

        var holidays = await GetHolidaysAsync(date.Year, cancellationToken);

        return !holidays.Contains(date);
    }

    private async Task<HashSet<DateOnly>> GetHolidaysAsync(int year, CancellationToken cancellationToken)
    {
        var cacheKey = $"{CacheKeyPrefix}:{year}";

        var cachedJson = await _cache.GetStringAsync(cacheKey, cancellationToken);

        if (!string.IsNullOrWhiteSpace(cachedJson))
        {
            var cachedDates = JsonSerializer.Deserialize<List<string>>(cachedJson);
            if (cachedDates is not null)
            {
                return cachedDates.Select(DateOnly.Parse).ToHashSet();
            }
        }

        var apiHolidays = await _brasilApiClient.GetHolidaysAsync(year, cancellationToken);

        var holidayDates = apiHolidays.Select(h => DateOnly.Parse(h.Date)).ToHashSet();

        var jsonToCache = JsonSerializer.Serialize(holidayDates.Select(d => d.ToString("yyyy-MM-dd")).ToList());

        await _cache.SetStringAsync(
            cacheKey,
            jsonToCache,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheTimeToLive
            },
            cancellationToken);

        return holidayDates;
    }
}