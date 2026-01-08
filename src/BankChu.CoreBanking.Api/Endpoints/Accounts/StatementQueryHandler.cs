using BankChu.CoreBanking.Api.Common.Cache;
using BankChu.CoreBanking.Api.Common.Errors;
using BankChu.CoreBanking.Api.Common.Http;
using BankChu.CoreBanking.Api.Contracts.Statements;
using BankChu.CoreBanking.Application.Statements;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace BankChu.CoreBanking.Api.Endpoints.Accounts;

public sealed class StatementQueryHandler
{
    private readonly GetStatementService _service;
    private readonly IDistributedCache _cache;

    public StatementQueryHandler(
        GetStatementService service,
        IDistributedCache cache)
    {
        _service = service;
        _cache = cache;
    }

    public async Task<IResult> HandleAsync(
        Guid accountId,
        DateTime from,
        DateTime to,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeys.Statement(accountId, from, to);

        // Cache
        var cached = await _cache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrWhiteSpace(cached))
        {
            var response = JsonSerializer.Deserialize<GetStatementResponse>(cached)!;
            return BuildResponse(response, accountId, from, to, httpContext);
        }

        // Use case
        var result = await _service.ExecuteAsync(
                                        new GetStatementQuery(accountId, from, to),
                                        cancellationToken);

        if (result.IsFailure)
            return result.Error!.ToProblemDetails(httpContext);

        var responseToCache = GetStatementResponse.FromResult(result.Value!);

        await _cache.SetStringAsync(
                        cacheKey,
                        JsonSerializer.Serialize(responseToCache),
                        new DistributedCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30)
                        },
                        cancellationToken);

        return BuildResponse(responseToCache, accountId, from, to, httpContext);
    }

    private static IResult BuildResponse(
        GetStatementResponse response,
        Guid accountId,
        DateTime from,
        DateTime to,
        HttpContext httpContext)
    {
        var etag = ETagGenerator.ForStatement(
            accountId,
            from,
            to,
            response.Items.Count,
            response.Items.LastOrDefault()?.OccurredAt);

        if (httpContext.Request.Headers.IfNoneMatch == etag)
            return Results.StatusCode(StatusCodes.Status304NotModified);

        httpContext.Response.Headers.ETag = etag;
        return Results.Ok(response);
    }
}