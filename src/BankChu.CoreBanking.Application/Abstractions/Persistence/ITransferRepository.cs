using BankChu.CoreBanking.Domain.Entities;

namespace BankChu.CoreBanking.Application.Abstractions.Persistence;

public interface ITransferRepository
{
    Task AddAsync(Transfer transfer, CancellationToken cancellationToken);

    Task<Transfer?> GetByIdAsync(Guid transferId, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(Guid transferId, CancellationToken cancellationToken);
}