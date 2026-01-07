namespace BankChu.CoreBanking.Application.Accounts.Create;

public sealed record CreateAccountResult(
    Guid Id,
    string Document,
    string Name,
    decimal Balance,
    bool IsActive,
    DateTime CreatedAt
);