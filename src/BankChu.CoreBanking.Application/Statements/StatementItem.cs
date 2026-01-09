using BankChu.CoreBanking.Application.Statements.Enum;

namespace BankChu.CoreBanking.Application.Statements;

public sealed record StatementItem(
    Guid TransferId,
    DateTime OccurredAt,
    decimal Amount,
    StatementType Type,
    Guid CounterpartyAccountId)
{
    public static StatementItem Credit(
        Guid transferId,
        DateTime occurredAt,
        decimal amount,
        Guid fromAccountId)
        => new(transferId, occurredAt, amount, StatementType.Credit, fromAccountId);

    public static StatementItem Debit(
        Guid transferId,
        DateTime occurredAt,
        decimal amount,
        Guid toAccountId)
        => new(transferId, occurredAt, amount, StatementType.Debit, toAccountId);
}