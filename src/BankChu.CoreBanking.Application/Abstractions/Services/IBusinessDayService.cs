namespace BankChu.CoreBanking.Application.Abstractions.Services;

public interface IBusinessDayService
{
    Task<bool> IsBusinessDayAsync(DateOnly date, CancellationToken cancellationToken);
}