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
                configuration.GetConnectionString("DefaultConnection"));
        });

        return services;
    }
}