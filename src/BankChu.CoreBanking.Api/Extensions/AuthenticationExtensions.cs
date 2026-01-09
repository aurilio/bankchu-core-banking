using BankChu.CoreBanking.Api.Auth;
using BankChu.CoreBanking.Api.Auth.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace BankChu.CoreBanking.Api.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.Configure<AuthOptions>(configuration.GetSection("Auth"));
        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        var jwtSection = configuration.GetSection("Jwt");
        var signingKey = jwtSection.GetValue<string>("SigningKey")!;

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = jwtSection.GetValue<string>("Issuer"),
                    ValidAudience = jwtSection.GetValue<string>("Audience"),
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(signingKey)),
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });

        return services;
    }
}