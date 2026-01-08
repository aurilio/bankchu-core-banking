using BankChu.CoreBanking.Application.Statements.Dto;

namespace BankChu.CoreBanking.Application.Abstractions.Persistence;

public interface IStatementRepository
{
    Task<IReadOnlyList<StatementItemDto>> GetAsync(
        Guid accountId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<StatementItemDto>> GetAfterAsync(
        Guid accountId,
        DateTime after,
        CancellationToken cancellationToken);
}