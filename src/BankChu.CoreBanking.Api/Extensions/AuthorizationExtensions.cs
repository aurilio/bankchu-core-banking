using BankChu.CoreBanking.Api.Auth;

namespace BankChu.CoreBanking.Api.Extensions;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddAuthorizationPolicies(
        this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("TransfersWrite", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireAssertion(ctx =>
                          ScopeAuthorizationHelper.HasScope(ctx, "transfers.write")));

            options.AddPolicy("StatementsRead", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireAssertion(ctx =>
                          ScopeAuthorizationHelper.HasScope(ctx, "statements.read")));

            options.AddPolicy("AccountsWrite", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireAssertion(ctx =>
                          ScopeAuthorizationHelper.HasScope(ctx, "accounts.write")));

            options.AddPolicy("AdminOnly", policy =>
                policy.RequireRole("admin"));
        });

        return services;
    }
}