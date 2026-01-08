using BankChu.CoreBanking.Api.Contracts.Statements;

namespace BankChu.CoreBanking.Api.Endpoints.Accounts;

public static class GetAccountStatementEndpoint
{
    public static void MapGetStatement(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/v1/accounts/{accountId:guid}/statements", GetStatementsAsync)
            .WithTags("Accounts")
            .Produces<GetStatementResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static Task<IResult> GetStatementsAsync(
        Guid accountId,
        DateTime from,
        DateTime to,
        StatementQueryHandler handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        return handler.HandleAsync(
            accountId,
            from,
            to,
            httpContext,
            cancellationToken);
    }
}