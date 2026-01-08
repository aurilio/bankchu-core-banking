using BankChu.CoreBanking.Infrastructure.External;
using BankChu.CoreBanking.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using NSubstitute;
using System.Text.Json;

namespace BankChu.CoreBanking.Infrastructure.Tests.Services;

public sealed class BusinessDayServiceTests
{
    private readonly IBrasilApiClient _brasilApiClient;
    private readonly IDistributedCache _cache;
    private readonly BusinessDayService _service;

    public BusinessDayServiceTests()
    {
        _brasilApiClient = Substitute.For<IBrasilApiClient>();
        _cache = Substitute.For<IDistributedCache>();

        _service = new BusinessDayService(_brasilApiClient, _cache);
    }

    [Fact]
    public async Task IsBusinessDayAsync_Should_Return_False_On_Weekend()
    {
        var saturday = new DateOnly(2025, 1, 4);

        var result = await _service.IsBusinessDayAsync(saturday, CancellationToken.None);

        result.Should().BeFalse();
        await _brasilApiClient.DidNotReceive().GetHolidaysAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task IsBusinessDayAsync_Should_Return_False_When_Is_Holiday()
    {
        var date = new DateOnly(2025, 1, 1); // exemplo
        var cacheKey = "holidays:2025";

        // Cache vazio -> vai bater na BrasilAPI
        _cache.GetStringAsync(cacheKey, Arg.Any<CancellationToken>())
            .Returns((string?)null);

        _brasilApiClient.GetHolidaysAsync(2025, Arg.Any<CancellationToken>())
            .Returns(new List<BrasilApiHolidayResponse>
            {
                new("2025-01-01", "Confraternização Universal")
            });

        var result = await _service.IsBusinessDayAsync(date, CancellationToken.None);

        result.Should().BeFalse();

        await _brasilApiClient.Received(1).GetHolidaysAsync(2025, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task IsBusinessDayAsync_Should_Use_Cache_When_Available()
    {
        var date = new DateOnly(2025, 1, 2);
        var cacheKey = "holidays:2025";

        var cached = JsonSerializer.Serialize(new List<string> { "2025-01-01" });

        _cache.GetStringAsync(cacheKey, Arg.Any<CancellationToken>())
            .Returns(cached);

        var result = await _service.IsBusinessDayAsync(date, CancellationToken.None);

        result.Should().BeTrue();

        await _brasilApiClient.DidNotReceive().GetHolidaysAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }
}
