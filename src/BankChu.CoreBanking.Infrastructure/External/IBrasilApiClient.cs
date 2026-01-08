using Refit;

namespace BankChu.CoreBanking.Infrastructure.External;

public interface IBrasilApiClient
{
    [Get("/api/feriados/v1/{year}")]
    Task<IReadOnlyList<BrasilApiHolidayResponse>> GetHolidaysAsync(int year, CancellationToken cancellationToken);
}