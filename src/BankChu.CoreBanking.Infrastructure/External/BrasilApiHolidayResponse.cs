namespace BankChu.CoreBanking.Infrastructure.External;

public sealed record BrasilApiHolidayResponse(
    string Date,
    string Name
);