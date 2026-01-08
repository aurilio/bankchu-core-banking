namespace BankChu.CoreBanking.Application.Statements;

public sealed record GetStatementResult(
    Guid AccountId,
    DateTime From,
    DateTime To,
    decimal OpeningBalance,
    decimal ClosingBalance,
    IReadOnlyList<StatementItem> Items
);