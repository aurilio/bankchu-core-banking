using BankChu.CoreBanking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BankChu.CoreBanking.Api.Extensions;

public static class MigrationExtensions
{
    public static void ApplyMigrations(this WebApplication app)
    {
        const int maxRetries = 10;
        const int delaySeconds = 5;

        for (var retry = 0; retry < maxRetries; retry++)
        {
            try
            {
                using var scope = app.Services.CreateScope();
                var dbContext = scope.ServiceProvider
                    .GetRequiredService<CoreBankingDbContext>();

                dbContext.Database.Migrate();
                break; // sucesso → sai do loop
            }
            catch (Exception ex)
            {
                if (retry == maxRetries - 1)
                    throw;

                Console.WriteLine(
                    $"Database not ready yet. Retry {retry + 1}/{maxRetries}. Error: {ex.Message}");

                Thread.Sleep(TimeSpan.FromSeconds(delaySeconds));
            }
        }
    }
}