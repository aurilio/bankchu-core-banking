namespace BankChu.CoreBanking.Api.Auth.Models;

public sealed class AuthUser
{
    public string Username { get; init; } = null!;

    public string Password { get; init; } = null!;

    public string[] Roles { get; init; } = Array.Empty<string>();
}