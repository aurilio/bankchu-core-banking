using BankChu.CoreBanking.Domain.Enums;

namespace BankChu.CoreBanking.Application.Transfers;

public sealed record TransferFundsResult(
    Guid TransferId,
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount,
    DateTime CreatedAt,
    TransferStatus Status
);