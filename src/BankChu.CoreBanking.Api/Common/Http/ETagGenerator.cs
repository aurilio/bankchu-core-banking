using System.Security.Cryptography;
using System.Text;

namespace BankChu.CoreBanking.Api.Common.Http;

public static class ETagGenerator
{
    public static string ForStatement(
        Guid accountId,
        DateTime from,
        DateTime to,
        int count,
        DateTime? lastOccurredAt)
    {
        var raw = $"{accountId}|{from:O}|{to:O}|{count}|{lastOccurredAt:O}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));

        return $"\"{Convert.ToHexString(bytes)}\"";
    }
}