using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace BankChu.CoreBanking.Infrastructure.Persistence;

public class CoreBankingDbContextFactory
    : IDesignTimeDbContextFactory<CoreBankingDbContext>
{
    public CoreBankingDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString =
            configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                "Connection string 'DefaultConnection' not found.");

        var optionsBuilder = new DbContextOptionsBuilder<CoreBankingDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new CoreBankingDbContext(optionsBuilder.Options);
    }
}