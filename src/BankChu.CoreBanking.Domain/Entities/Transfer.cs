namespace BankChu.CoreBanking.Domain.Entities;

public class Transfer
{
    public Guid Id { get; private set; }

    public Guid FromAccountId { get; private set; }

    public Guid ToAccountId { get; private set; }

    public decimal Amount { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public string Status { get; private set; } = null!;

    public string? Reason { get; private set; }

    protected Transfer() { } // EF

    public Transfer(Guid fromAccountId, Guid toAccountId, decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero");

        Id = Guid.NewGuid();
        FromAccountId = fromAccountId;
        ToAccountId = toAccountId;
        Amount = amount;
        CreatedAt = DateTime.UtcNow;
        Status = "Completed";
    }

    public void Reject(string reason)
    {
        Status = "Rejected";
        Reason = reason;
    }
}