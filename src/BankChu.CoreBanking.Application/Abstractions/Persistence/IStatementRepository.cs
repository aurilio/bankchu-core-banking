using BankChu.CoreBanking.Application.Statements;

namespace BankChu.CoreBanking.Application.Abstractions.Persistence;

public interface IStatementRepository
{
    Task<IReadOnlyList<StatementItem>> GetAsync(
        Guid accountId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}