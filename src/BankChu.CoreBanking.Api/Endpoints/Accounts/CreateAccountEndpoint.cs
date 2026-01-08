using BankChu.CoreBanking.Api.Common.Errors;
using BankChu.CoreBanking.Api.Contracts.Accounts;
using BankChu.CoreBanking.Application.Accounts.Create;

namespace BankChu.CoreBanking.Api.Endpoints.Accounts;

public static class AccountsEndpoints
{
    public static void MapAccountsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/v1/accounts", CreateAccountAsync)
            .RequireAuthorization("AccountsWrite")
            .WithTags("Accounts")
            .Produces<CreateAccountResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);
    }

    private static async Task<IResult> CreateAccountAsync(
        CreateAccountRequest request,
        CreateAccountService createAccountService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var command = new CreateAccountCommand(
            request.Document,
            request.Name,
            request.InitialBalance);

        var result = await createAccountService.ExecuteAsync(command, cancellationToken);

        if (result.IsFailure)
            return result.Error!.ToProblemDetails(httpContext);

        var value = result.Value!;

        var response = new CreateAccountResponse(
            value.Id,
            value.Document,
            value.Name,
            value.Balance,
            value.IsActive,
            value.CreatedAt);

        return Results.Created($"/api/v1/accounts/{response.Id}", response);
    }
}