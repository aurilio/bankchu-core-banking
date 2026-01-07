namespace BankChu.CoreBanking.Api.Contracts.Accounts;

public sealed record CreateAccountResponse(
    Guid Id,
    string Document,
    string Name,
    decimal Balance,
    bool IsActive,
    DateTime CreatedAt
);