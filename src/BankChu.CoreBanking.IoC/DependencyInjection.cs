using BankChu.CoreBanking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BankChu.CoreBanking.IoC;

public static class DependencyInjection
{
    public static IServiceCollection AddCoreBanking(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<CoreBankingDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql =>
                {
                    sql.MigrationsAssembly(typeof(CoreBankingDbContext).Assembly.FullName);
                    sql.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null);
                });

        });

        return services;
    }
}