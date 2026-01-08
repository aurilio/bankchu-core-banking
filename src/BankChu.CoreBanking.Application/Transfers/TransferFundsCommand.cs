namespace BankChu.CoreBanking.Application.Transfers;

public sealed record TransferFundsCommand(
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount,
    DateOnly RequestedDate,
    string IdempotencyKey
);