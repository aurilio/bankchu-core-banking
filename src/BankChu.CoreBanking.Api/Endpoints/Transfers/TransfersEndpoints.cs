using BankChu.CoreBanking.Api.Common.Errors;
using BankChu.CoreBanking.Api.Contracts.Transfers;
using BankChu.CoreBanking.Application.Common.Errors;
using BankChu.CoreBanking.Application.Transfers;

namespace BankChu.CoreBanking.Api.Endpoints.Transfers;

public static class TransfersEndpoints
{
    private const string IdempotencyHeaderName = "Idempotency-Key";

    public static void MapTransfersEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/v1/transfers", TransferAsync)
            .RequireAuthorization("TransfersWrite")
            .WithTags("Transfers")
            .Produces<TransferFundsResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);
    }

    private static async Task<IResult> TransferAsync(
        TransferFundsRequest request,
        HttpContext httpContext,
        TransferFundsService service,
        CancellationToken cancellationToken)
    {
        if (!httpContext.Request.Headers.TryGetValue(IdempotencyHeaderName, out var idempotencyKeyValues))
        {
            return CommonErrors
                .BadRequest(
                    "IDEMPOTENCY_KEY_REQUIRED",
                    $"Header '{IdempotencyHeaderName}' is required.")
                .ToProblemDetails(httpContext);
        }

        var idempotencyKey = idempotencyKeyValues.ToString();

        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            return CommonErrors
                .BadRequest(
                    "IDEMPOTENCY_KEY_INVALID",
                    $"Header '{IdempotencyHeaderName}' cannot be empty.")
                .ToProblemDetails(httpContext);
        }

        var command = new TransferFundsCommand(
            request.FromAccountId,
            request.ToAccountId,
            request.Amount,
            RequestedDate: DateOnly.FromDateTime(DateTime.UtcNow),
            IdempotencyKey: idempotencyKey);

        var result = await service.ExecuteAsync(command, cancellationToken);

        if (result.IsFailure)
            return result.Error!.ToProblemDetails(httpContext);

        var value = result.Value!;

        var response = new TransferFundsResponse(
            value.TransferId,
            value.FromAccountId,
            value.ToAccountId,
            value.Amount,
            value.CreatedAt,
            value.Status);

        return Results.Created($"/api/v1/transfers/{response.TransferId}", response);
    }
}