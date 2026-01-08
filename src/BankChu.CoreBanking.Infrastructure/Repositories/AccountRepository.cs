using BankChu.CoreBanking.Application.Abstractions.Persistence;
using BankChu.CoreBanking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BankChu.CoreBanking.Infrastructure.Persistence.Repositories;

public sealed class AccountRepository : IAccountRepository
{
    private readonly CoreBankingDbContext _dbContext;

    public AccountRepository(CoreBankingDbContext dbContext)
    {
        this._dbContext = dbContext;
    }

    public async Task<Account?> GetByIdAsync(Guid accountId, CancellationToken cancellationToken)
    {
        return await _dbContext.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(account => account.Id == accountId, cancellationToken);
    }

    public async Task<bool> ExistsByDocumentAsync(string document, CancellationToken cancellationToken)
    {
        return await _dbContext.Accounts.AnyAsync(account => account.Document == document, cancellationToken);
    }

    public async Task AddAsync(Account account, CancellationToken cancellationToken)
    {
        await _dbContext.Accounts.AddAsync(account, cancellationToken);
    }
}