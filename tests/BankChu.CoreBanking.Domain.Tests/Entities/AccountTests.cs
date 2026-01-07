using BankChu.CoreBanking.Domain.Entities;
using FluentAssertions;

namespace BankChu.CoreBanking.Domain.Tests.Entities;

public sealed class AccountTests
{
    [Fact]
    public void Constructor_Should_Create_Account_With_Valid_Initial_State()
    {
        // Arrange
        var document = "12345678900";
        var name = "John Doe";
        var initialBalance = 1000m;

        // Act
        var account = new Account(document, name, initialBalance);

        // Assert
        account.Id.Should().NotBeEmpty();
        account.Document.Should().Be(document);
        account.Name.Should().Be(name);
        account.Balance.Should().Be(initialBalance);
        account.IsActive.Should().BeTrue();
        account.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Constructor_Should_Throw_When_Initial_Balance_Is_Negative()
    {
        // Arrange
        var document = "12345678900";
        var name = "John Doe";
        var invalidBalance = -1m;

        // Act
        var act = () => new Account(document, name, invalidBalance);

        // Assert
        act.Should()
           .Throw<ArgumentException>()
           .WithMessage("Initial balance cannot be negative");
    }

    [Fact]
    public void Debit_Should_Decrease_Balance_When_Amount_Is_Valid()
    {
        // Arrange
        var account = new Account("12345678900", "John Doe", 1000m);

        // Act
        account.Debit(200m);

        // Assert
        account.Balance.Should().Be(800m);
    }

    [Fact]
    public void Debit_Should_Throw_When_Amount_Is_Zero_Or_Negative()
    {
        // Arrange
        var account = new Account("12345678900", "John Doe", 1000m);

        // Act
        var act = () => account.Debit(0);

        // Assert
        act.Should()
           .Throw<ArgumentException>()
           .WithMessage("Amount must be greater than zero");
    }

    [Fact]
    public void Debit_Should_Throw_When_Insufficient_Balance()
    {
        // Arrange
        var account = new Account("12345678900", "John Doe", 100m);

        // Act
        var act = () => account.Debit(200m);

        // Assert
        act.Should()
           .Throw<InvalidOperationException>()
           .WithMessage("Insufficient balance");
    }

    [Fact]
    public void Credit_Should_Increase_Balance_When_Amount_Is_Valid()
    {
        // Arrange
        var account = new Account("12345678900", "John Doe", 500m);

        // Act
        account.Credit(300m);

        // Assert
        account.Balance.Should().Be(800m);
    }

    [Fact]
    public void Credit_Should_Throw_When_Amount_Is_Zero_Or_Negative()
    {
        // Arrange
        var account = new Account("12345678900", "John Doe", 500m);

        // Act
        var act = () => account.Credit(0);

        // Assert
        act.Should()
           .Throw<ArgumentException>()
           .WithMessage("Amount must be greater than zero");
    }

    [Fact]
    public void Deactivate_Should_Set_IsActive_To_False()
    {
        // Arrange
        var account = new Account("12345678900", "John Doe", 100m);

        // Act
        account.Deactivate();

        // Assert
        account.IsActive.Should().BeFalse();
    }
}