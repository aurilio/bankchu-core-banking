namespace BankChu.CoreBanking.Api.Contracts.Transfers;

public sealed record TransferFundsRequest(
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount
);