using BankChu.CoreBanking.Application.Statements;
using BankChu.CoreBanking.Application.Statements.Enum;

namespace BankChu.CoreBanking.Api.Contracts.Statements;

public sealed record StatementItemResponse(
    Guid TransferId,
    DateTime OccurredAt,
    decimal Amount,
    StatementType Type,
    Guid CounterpartyAccountId,
    decimal BalanceAfter)
{
    public static StatementItemResponse From(StatementItem item)
        => new(
            item.TransferId,
            item.OccurredAt,
            item.Amount,
            item.Type,
            item.CounterpartyAccountId,
            item.BalanceAfter);
}