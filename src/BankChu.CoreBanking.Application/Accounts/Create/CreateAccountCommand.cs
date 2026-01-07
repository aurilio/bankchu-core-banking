namespace BankChu.CoreBanking.Application.Accounts.Create;

public sealed record CreateAccountCommand(
    string Document,
    string Name,
    decimal InitialBalance
);