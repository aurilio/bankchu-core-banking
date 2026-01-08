using BankChu.CoreBanking.Application.Statements;

namespace BankChu.CoreBanking.Api.Contracts.Statements;

public sealed record GetStatementResponse(
    Guid AccountId,
    DateTime From,
    DateTime To,
    decimal OpeningBalance,
    decimal ClosingBalance,
    IReadOnlyList<StatementItemResponse> Items)
{
    public static GetStatementResponse FromResult(GetStatementResult result)
        => new(
            result.AccountId,
            result.From,
            result.To,
            result.OpeningBalance,
            result.ClosingBalance,
            result.Items.Select(StatementItemResponse.From).ToList());
}