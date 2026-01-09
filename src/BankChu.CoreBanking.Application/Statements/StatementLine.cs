using BankChu.CoreBanking.Application.Statements.Enum;

namespace BankChu.CoreBanking.Application.Statements;

public sealed record StatementLine(
    Guid TransferId,
    DateTime OccurredAt,
    decimal Amount,
    StatementType Type,
    Guid CounterpartyAccountId,
    decimal BalanceAfter);
