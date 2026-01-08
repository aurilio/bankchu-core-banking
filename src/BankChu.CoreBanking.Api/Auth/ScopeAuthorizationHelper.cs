using Microsoft.AspNetCore.Authorization;

namespace BankChu.CoreBanking.Api.Auth;

public static class ScopeAuthorizationHelper
{
    public static bool HasScope(AuthorizationHandlerContext context, string scope)
    {
        var scopeClaim = context.User.FindFirst("scope")?.Value;

        if (string.IsNullOrWhiteSpace(scopeClaim))
            return false;

        return scopeClaim
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Contains(scope, StringComparer.OrdinalIgnoreCase);
    }
}
