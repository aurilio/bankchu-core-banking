using BankChu.CoreBanking.Domain.Entities;

namespace BankChu.CoreBanking.Application.Abstractions.Persistence;

public interface IAccountRepository
{
    Task<bool> ExistsByDocumentAsync(string document, CancellationToken cancellationToken);

    Task AddAsync(Account account, CancellationToken cancellationToken);
}