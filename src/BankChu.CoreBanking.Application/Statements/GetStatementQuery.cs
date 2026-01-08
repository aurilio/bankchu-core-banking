namespace BankChu.CoreBanking.Application.Statements;

public sealed record GetStatementQuery(
    Guid AccountId,
    DateTime From,
    DateTime To
);
