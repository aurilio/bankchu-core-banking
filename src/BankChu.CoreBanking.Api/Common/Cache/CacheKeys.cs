namespace BankChu.CoreBanking.Api.Common.Cache;

public static class CacheKeys
{
    public static string Statement(
        Guid accountId,
        DateTime from,
        DateTime to)
        => $"statement:{accountId}:{from:yyyyMMdd}:{to:yyyyMMdd}";
}