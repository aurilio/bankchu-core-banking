using BankChu.CoreBanking.Domain.Enums;

namespace BankChu.CoreBanking.Api.Contracts.Transfers;

public sealed record TransferFundsResponse(
    Guid TransferId,
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount,
    DateTime CreatedAt,
    TransferStatus Status
);