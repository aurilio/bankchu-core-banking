using BankChu.CoreBanking.Application.Abstractions.Persistence;
using BankChu.CoreBanking.Application.Statements.Dto;
using BankChu.CoreBanking.Application.Statements.Enum;
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

    public async Task<IReadOnlyList<StatementItemDto>> GetAsync(
        Guid accountId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        return await _context.Transfers
            .AsNoTracking()
            .Where(t =>
                t.Status == Domain.Enums.TransferStatus.Completed &&
                t.CreatedAt >= from &&
                t.CreatedAt <= to &&
                (t.FromAccountId == accountId || t.ToAccountId == accountId))
            .Select(t => new StatementItemDto(
                t.Id,
                t.CreatedAt,
                t.Amount,
                t.ToAccountId == accountId
                    ? StatementType.Credit
                    : StatementType.Debit,
                t.ToAccountId == accountId
                    ? t.FromAccountId
                    : t.ToAccountId))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StatementItemDto>> GetAfterAsync(
        Guid accountId,
        DateTime after,
        CancellationToken cancellationToken)
    {
        return await _context.Transfers
            .AsNoTracking()
            .Where(t =>
                t.Status == TransferStatus.Completed &&
                t.CreatedAt > after &&
                (t.FromAccountId == accountId || t.ToAccountId == accountId))
            .Select(t => new StatementItemDto(
                t.Id,
                t.CreatedAt,
                t.Amount,
                t.ToAccountId == accountId
                    ? StatementType.Credit
                    : StatementType.Debit,
                t.ToAccountId == accountId
                    ? t.FromAccountId
                    : t.ToAccountId))
            .ToListAsync(cancellationToken);
    }
}