using BankChu.CoreBanking.Application.Abstractions.Persistence;
using BankChu.CoreBanking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BankChu.CoreBanking.Infrastructure.Persistence.Repositories;

public sealed class AccountRepository : IAccountRepository
{
    private readonly CoreBankingDbContext dbContext;

    public AccountRepository(CoreBankingDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<bool> ExistsByDocumentAsync(
        string document,
        CancellationToken cancellationToken)
    {
        return await dbContext.Accounts
            .AnyAsync(
                account => account.Document == document,
                cancellationToken);
    }

    public async Task AddAsync(
        Account account,
        CancellationToken cancellationToken)
    {
        await dbContext.Accounts.AddAsync(account, cancellationToken);
    }
}