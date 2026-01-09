using BankChu.CoreBanking.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BankChu.CoreBanking.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly CoreBankingDbContext _dbContext;
    private readonly ILogger<UnitOfWork> _logger;

    public UnitOfWork(
        CoreBankingDbContext dbContext,
        ILogger<UnitOfWork> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task ExecuteAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken)
    {
        var strategy = _dbContext.Database.CreateExecutionStrategy();

        _logger.LogDebug("Starting UnitOfWork execution using execution strategy {StrategyType}", strategy.GetType().Name);

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            _logger.LogDebug("Database transaction started");

            try
            {
                await operation(cancellationToken);

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                
                _logger.LogDebug("Database transaction committed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Database transaction rolled back due to an exception");

                await transaction.RollbackAsync(cancellationToken);

                throw;
            }
        });
    }

    public async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken)
    {
        var strategy = _dbContext.Database.CreateExecutionStrategy();

        _logger.LogDebug("Starting UnitOfWork<T> execution using execution strategy {StrategyType}", strategy.GetType().Name);

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            _logger.LogDebug("Database transaction started");

            try
            {
                var result = await operation(cancellationToken);

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogDebug("Database transaction committed successfully");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Database transaction rolled back due to an exception");

                await transaction.RollbackAsync(cancellationToken);

                throw;
            }
        });
    }
}
