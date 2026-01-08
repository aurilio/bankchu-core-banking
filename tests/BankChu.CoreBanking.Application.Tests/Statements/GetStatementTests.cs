using BankChu.CoreBanking.Application.Abstractions.Persistence;
using BankChu.CoreBanking.Application.Common.Errors;
using BankChu.CoreBanking.Application.Statements;
using BankChu.CoreBanking.Application.Statements.Dto;
using BankChu.CoreBanking.Application.Statements.Enum;
using BankChu.CoreBanking.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace BankChu.CoreBanking.Application.Tests.Statements;

public sealed class GetStatementServiceTests
{
    private readonly IAccountRepository _accountRepository;
    private readonly IStatementRepository _statementRepository;
    private readonly GetStatementService _service;

    public GetStatementServiceTests()
    {
        _accountRepository = Substitute.For<IAccountRepository>();
        _statementRepository = Substitute.For<IStatementRepository>();

        _service = new GetStatementService(_accountRepository, _statementRepository);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldFail_WhenAccountDoesNotExist()
    {
        // Arrange
        var accountId = Guid.NewGuid();

        _accountRepository
            .GetByIdAsync(accountId, Arg.Any<CancellationToken>())
            .Returns((Account?)null);

        var query = new GetStatementQuery(accountId, DateTime.UtcNow.AddDays(-10), DateTime.UtcNow);

        // Act
        var result = await _service.ExecuteAsync(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AccountErrors.NotFound(accountId));
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnEmptyStatement_WhenNoTransfers()
    {
        // Arrange
        var account = new Account("123", "Test", 1000m);

        _accountRepository
            .GetByIdAsync(account.Id, Arg.Any<CancellationToken>())
            .Returns(account);

        _statementRepository
            .GetAfterAsync(account.Id, Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<StatementItemDto>());

        _statementRepository
            .GetAsync(account.Id, Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<StatementItemDto>());

        var query = new GetStatementQuery(account.Id, DateTime.UtcNow.AddDays(-30), DateTime.UtcNow.AddDays(-1));

        // Act
        var result = await _service.ExecuteAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var statement = result.Value!;
        statement.Items.Should().BeEmpty();
        statement.OpeningBalance.Should().Be(1000m);
        statement.ClosingBalance.Should().Be(1000m);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCalculateBalancesCorrectly()
    {
        // Arrange
        var account = new Account("123", "Test", 1000m);
        var counterparty = Guid.NewGuid();

        _accountRepository
            .GetByIdAsync(account.Id, Arg.Any<CancellationToken>())
            .Returns(account);

        _statementRepository
            .GetAfterAsync(account.Id, Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new[]
            {
            new StatementItemDto(
                Guid.NewGuid(),
                DateTime.UtcNow.AddDays(1),
                200m,
                StatementType.Credit,
                counterparty)
            });

        _statementRepository
            .GetAsync(account.Id, Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new[]
            {
            new StatementItemDto(
                Guid.NewGuid(),
                DateTime.UtcNow.AddDays(-5),
                100m,
                StatementType.Debit,
                counterparty),

            new StatementItemDto(
                Guid.NewGuid(),
                DateTime.UtcNow.AddDays(-3),
                50m,
                StatementType.Credit,
                counterparty)
            });

        var query = new GetStatementQuery(account.Id, DateTime.UtcNow.AddDays(-10), DateTime.UtcNow.AddDays(-1));

        // Act
        var result = await _service.ExecuteAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var statement = result.Value!;
        statement.ClosingBalance.Should().Be(800m);
        statement.OpeningBalance.Should().Be(850m);

        statement.Items.Should().HaveCount(2);
        statement.Items.Last().BalanceAfter.Should().Be(800m);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldOrderItemsAndCalculateRunningBalance()
    {
        // Arrange
        var account = new Account("123", "Test", 500m);
        var other = Guid.NewGuid();

        _accountRepository
            .GetByIdAsync(account.Id, Arg.Any<CancellationToken>())
            .Returns(account);

        _statementRepository
            .GetAfterAsync(account.Id, Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<StatementItemDto>());

        _statementRepository
            .GetAsync(account.Id, Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new[]
            {
            new StatementItemDto(
                Guid.NewGuid(),
                new DateTime(2025, 01, 05),
                50m,
                StatementType.Credit,
                other),

            new StatementItemDto(
                Guid.NewGuid(),
                new DateTime(2025, 01, 02),
                100m,
                StatementType.Debit,
                other)
            });

        var query = new GetStatementQuery(account.Id, new DateTime(2025, 01, 01), new DateTime(2025, 01, 31));

        // Act
        var result = await _service.ExecuteAsync(query, CancellationToken.None);

        // Assert
        var items = result.Value!.Items;

        items[0].OccurredAt.Should().Be(new DateTime(2025, 01, 02));
        items[0].BalanceAfter.Should().Be(450m);

        items[1].OccurredAt.Should().Be(new DateTime(2025, 01, 05));
        items[1].BalanceAfter.Should().Be(500m);
    }
}