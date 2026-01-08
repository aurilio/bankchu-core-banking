namespace BankChu.CoreBanking.Api.Auth;

public interface IJwtTokenService
{
    string CreateToken(string username, IEnumerable<string> roles, IEnumerable<string> scopes);
}