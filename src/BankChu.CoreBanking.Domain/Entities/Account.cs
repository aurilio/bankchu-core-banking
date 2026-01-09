namespace BankChu.CoreBanking.Domain.Entities;

public class Account
{
    public Guid Id { get; private set; }

    public string Document { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public decimal InitialBalance { get; private set; }

    public decimal Balance { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    protected Account() { } // EF

    public Account(string document, string name, decimal initialBalance)
    {
        if (initialBalance < 0)
            throw new ArgumentException("Initial balance cannot be negative");

        Id = Guid.NewGuid();
        Document = document;
        Name = name;
        InitialBalance = initialBalance;
        Balance = initialBalance;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void Debit(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero");

        if (Balance < amount)
            throw new InvalidOperationException("Insufficient balance");

        Balance -= amount;
    }

    public void Credit(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero");

        Balance += amount;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}