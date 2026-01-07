using BankChu.CoreBanking.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore.Storage;

namespace BankChu.CoreBanking.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly CoreBankingDbContext dbContext;
    private IDbContextTransaction? currentTransaction;

    public UnitOfWork(CoreBankingDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken)
    {
        currentTransaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
        await currentTransaction!.CommitAsync(cancellationToken);
    }

    public async Task RollbackAsync(CancellationToken cancellationToken)
    {
        await currentTransaction!.RollbackAsync(cancellationToken);
    }
}