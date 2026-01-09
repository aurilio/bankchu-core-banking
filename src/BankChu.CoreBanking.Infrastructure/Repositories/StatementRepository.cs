using BankChu.CoreBanking.Application.Abstractions.Persistence;
using BankChu.CoreBanking.Application.Statements;
using BankChu.CoreBanking.Domain.Enums;
using BankChu.CoreBanking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BankChu.CoreBanking.Infrastructure.Repositories;

public sealed class StatementRepository : IStatementRepository
{
    private readonly CoreBankingDbContext _context;

    public StatementRepository(CoreBankingDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<StatementItem>> GetAsync(
        Guid accountId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        return await _context.Transfers
            .AsNoTracking()
            .Where(t =>
                t.Status == TransferStatus.Completed &&
                t.CreatedAt >= from &&
                t.CreatedAt <= to &&
                (t.FromAccountId == accountId || t.ToAccountId == accountId))
            .OrderBy(t => t.CreatedAt)
            .Select(t =>
                t.ToAccountId == accountId
                    ? StatementItem.Credit(t.Id, t.CreatedAt, t.Amount, t.FromAccountId)
                    : StatementItem.Debit(t.Id, t.CreatedAt, t.Amount, t.ToAccountId))
            .ToListAsync(cancellationToken);
    }
}