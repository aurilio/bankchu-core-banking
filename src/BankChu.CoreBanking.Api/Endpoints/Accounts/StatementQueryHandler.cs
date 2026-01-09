using BankChu.CoreBanking.Api.Common.Cache;
using BankChu.CoreBanking.Api.Common.Errors;
using BankChu.CoreBanking.Api.Common.Http;
using BankChu.CoreBanking.Api.Contracts.Statements;
using BankChu.CoreBanking.Application.Statements;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace BankChu.CoreBanking.Api.Endpoints.Accounts;

public sealed class StatementQueryHandler
{
    private readonly GetStatementService _service;
    private readonly IDistributedCache _cache;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ILogger<StatementQueryHandler> _logger;

    public StatementQueryHandler(
        GetStatementService service,
        IDistributedCache cache,
        IOptions<JsonOptions> jsonOptions,
        ILogger<StatementQueryHandler> logger)
    {
        _service = service;
        _cache = cache;
        _jsonOptions = jsonOptions.Value.SerializerOptions;
        _logger = logger;
    }

    public async Task<IResult> HandleAsync(
        Guid accountId,
        DateTime from,
        DateTime to,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Statement request received. AccountId={AccountId}, From={From}, To={To}",
            accountId,
            from,
            to);

        var cacheKey = CacheKeys.Statement(accountId, from, to);

        var cachedBytes = await _cache.GetAsync(cacheKey, cancellationToken);
        if (cachedBytes is not null)
        {
            _logger.LogDebug(
                "Statement cache HIT. AccountId={AccountId}, CacheKey={CacheKey}",
                accountId,
                cacheKey);

            var response = JsonSerializer.Deserialize<GetStatementResponse>(cachedBytes, _jsonOptions)!;

            return BuildResponse(response, accountId, from, to, httpContext, _logger);
        }

        _logger.LogDebug(
            "Statement cache MISS. AccountId={AccountId}, CacheKey={CacheKey}",
            accountId,
            cacheKey);

        var result = await _service.ExecuteAsync(
                                        new GetStatementQuery(accountId, from, to),
                                        cancellationToken);

        if (result.IsFailure)
        {
            _logger.LogWarning(
                "Statement request failed. AccountId={AccountId}, ErrorCode={ErrorCode}",
                accountId,
                result.Error!.Code);

            return result.Error!.ToProblemDetails(httpContext);
        }

        var responseToCache = GetStatementResponse.FromResult(result.Value!);

        await _cache.SetAsync(
                        cacheKey,
                        JsonSerializer.SerializeToUtf8Bytes(responseToCache, _jsonOptions),
                        new DistributedCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30)
                        },
                        cancellationToken);

        _logger.LogDebug(
            "Statement cached successfully. AccountId={AccountId}, CacheKey={CacheKey}",
            accountId,
            cacheKey);

        return BuildResponse(responseToCache, accountId, from, to, httpContext, _logger);
    }

    private static IResult BuildResponse(
        GetStatementResponse response,
        Guid accountId,
        DateTime from,
        DateTime to,
        HttpContext httpContext,
        ILogger<StatementQueryHandler> _logger)
    {
        var etag = ETagGenerator.ForStatement(
            accountId,
            from,
            to,
            response.Items.Count,
            response.Items.LastOrDefault()?.OccurredAt);

        if (httpContext.Request.Headers.IfNoneMatch == etag)
        {
            _logger.LogInformation(
                "Statement response not modified (ETag matched). AccountId={AccountId}, ETag={ETag}",
                accountId,
                etag);

            return Results.StatusCode(StatusCodes.Status304NotModified);
        }

        httpContext.Response.Headers.ETag = etag;

        _logger.LogDebug(
            "Statement response returned with ETag. AccountId={AccountId}, ETag={ETag}",
            accountId,
            etag);

        return Results.Ok(response);
    }
}