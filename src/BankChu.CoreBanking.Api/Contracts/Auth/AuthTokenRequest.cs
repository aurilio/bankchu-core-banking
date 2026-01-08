namespace BankChu.CoreBanking.Api.Contracts.Auth;

public sealed record AuthTokenRequest(
    string Username,
    string Password
);