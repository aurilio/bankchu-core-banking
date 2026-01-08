namespace BankChu.CoreBanking.Api.Auth.Models;

public sealed class AuthOptions
{
    public List<AuthUser> Users { get; init; } = new();
}