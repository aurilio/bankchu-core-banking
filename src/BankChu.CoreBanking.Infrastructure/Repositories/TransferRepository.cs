using BankChu.CoreBanking.Application.Abstractions.Persistence;
using BankChu.CoreBanking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BankChu.CoreBanking.Infrastructure.Persistence.Repositories;

public sealed class TransferRepository : ITransferRepository
{
    private readonly CoreBankingDbContext _dbContext;

    public TransferRepository(CoreBankingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Transfer transfer, CancellationToken cancellationToken)
    {
        await _dbContext.Transfers.AddAsync(transfer, cancellationToken);
    }

    public async Task<Transfer?> GetByIdAsync(Guid transferId, CancellationToken cancellationToken)
    {
        return await _dbContext.Transfers
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == transferId, cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid transferId, CancellationToken cancellationToken)
    {
        return await _dbContext.Transfers.AnyAsync(t => t.Id == transferId, cancellationToken);
    }
}