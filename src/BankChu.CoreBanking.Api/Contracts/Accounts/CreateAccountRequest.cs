namespace BankChu.CoreBanking.Api.Contracts.Accounts;

public sealed record CreateAccountRequest(
    string Document,
    string Name,
    decimal InitialBalance
);