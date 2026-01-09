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
    public static StatementItemResponse From(StatementLine line)
        => new(
            line.TransferId,
            line.OccurredAt,
            line.Amount,
            line.Type,
            line.CounterpartyAccountId,
            line.BalanceAfter);
}