using BankChu.CoreBanking.Api.Auth;
using BankChu.CoreBanking.Api.Auth.Models;
using BankChu.CoreBanking.Api.Contracts.Auth;
using Microsoft.Extensions.Options;

namespace BankChu.CoreBanking.Api.Endpoints.Auth;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/v1/auth/token", TokenAsync)
            .WithTags("Auth")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static IResult TokenAsync(
        AuthTokenRequest request,
        IOptions<AuthOptions> authOptions,
        IJwtTokenService tokenService)
    {
        var user = authOptions.Value.Users.FirstOrDefault(u =>
            string.Equals(u.Username, request.Username, StringComparison.OrdinalIgnoreCase) &&
            u.Password == request.Password);

        if (user is null)
            return Results.Unauthorized();

        var scopes = user.Roles.Contains("admin", StringComparer.OrdinalIgnoreCase)
            ? new[] { "accounts.write", "transfers.write", "statements.read" }
            : new[] { "transfers.write", "statements.read" };

        var token = tokenService.CreateToken(user.Username, user.Roles, scopes);

        return Results.Ok(new AuthTokenResponse(token));
    }
}