using BankChu.CoreBanking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BankChu.CoreBanking.Infrastructure.Persistence;

public class CoreBankingDbContext : DbContext
{
    public CoreBankingDbContext(DbContextOptions<CoreBankingDbContext> options)
        : base(options) { }

    public DbSet<Account> Accounts => Set<Account>();

    public DbSet<Transfer> Transfers => Set<Transfer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Document).IsRequired();
            entity.HasIndex(x => x.Document).IsUnique();
            entity.Property(x => x.Balance).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Transfer>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Amount).HasPrecision(18, 2);
        });
    }
}
