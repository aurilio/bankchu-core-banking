using BankChu.CoreBanking.Application.Abstractions.Persistence;
using BankChu.CoreBanking.Application.Accounts.Create;
using BankChu.CoreBanking.Application.Common.Errors;
using BankChu.CoreBanking.Application.Statements;
using BankChu.CoreBanking.Application.Statements.Enum;
using BankChu.CoreBanking.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace BankChu.CoreBanking.Application.Tests.Statements;

public sealed class GetStatementServiceTests
{
    private readonly IAccountRepository _accountRepository;
    private readonly IStatementRepository _statementRepository;
    private readonly ILogger<GetStatementService> _logger;
    private readonly GetStatementService _service;

    public GetStatementServiceTests()
    {
        _accountRepository = Substitute.For<IAccountRepository>();
        _statementRepository = Substitute.For<IStatementRepository>();
        _logger = Substitute.For<ILogger<GetStatementService>>();

        _service = new GetStatementService(_accountRepository, _statementRepository, _logger);
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
    public async Task ExecuteAsync_ShouldReturnEmptyStatement_WhenNoTransfersInPeriod()
    {
        // Arrange
        var account = new Account("123", "Test", 1000m);

        _accountRepository
            .GetByIdAsync(account.Id, Arg.Any<CancellationToken>())
            .Returns(account);

        _statementRepository
            .GetAsync(account.Id, Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<StatementItem>());

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
    public async Task ExecuteAsync_ShouldCalculateOpeningBalance_ByReversingPeriodMovements()
    {
        // Arrange
        // Saldo atual (closing) = 1000
        // No período: -100 (debit) e +50 (credit)
        // Então opening = 1000 +100 -50 = 1050
        var account = new Account("123", "Test", 1000m);
        var counterparty = Guid.NewGuid();

        _accountRepository
            .GetByIdAsync(account.Id, Arg.Any<CancellationToken>())
            .Returns(account);

        var itemsInPeriod = new[]
        {
            new StatementItem(
                TransferId: Guid.NewGuid(),
                OccurredAt: DateTime.UtcNow.AddDays(-5),
                Amount: 100m,
                Type: StatementType.Debit,
                CounterpartyAccountId: counterparty),

            new StatementItem(
                TransferId: Guid.NewGuid(),
                OccurredAt: DateTime.UtcNow.AddDays(-3),
                Amount: 50m,
                Type: StatementType.Credit,
                CounterpartyAccountId: counterparty),
        };

        _statementRepository
            .GetAsync(account.Id, Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(itemsInPeriod);

        var query = new GetStatementQuery(account.Id, DateTime.UtcNow.AddDays(-10), DateTime.UtcNow.AddDays(-1));

        // Act
        var result = await _service.ExecuteAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var statement = result.Value!;
        statement.ClosingBalance.Should().Be(1000m);
        statement.OpeningBalance.Should().Be(1050m);
        statement.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnItems_AsProvidedByRepository()
    {
        // Arrange
        var account = new Account("123", "Test", 500m);
        var other = Guid.NewGuid();

        _accountRepository
            .GetByIdAsync(account.Id, Arg.Any<CancellationToken>())
            .Returns(account);

        var itemsInPeriod = new[]
        {
            new StatementItem(
                TransferId: Guid.NewGuid(),
                OccurredAt: new DateTime(2025, 01, 05),
                Amount: 50m,
                Type: StatementType.Credit,
                CounterpartyAccountId: other),

            new StatementItem(
                TransferId: Guid.NewGuid(),
                OccurredAt: new DateTime(2025, 01, 02),
                Amount: 100m,
                Type: StatementType.Debit,
                CounterpartyAccountId: other),
        };

        _statementRepository
            .GetAsync(account.Id, Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(itemsInPeriod);

        var query = new GetStatementQuery(account.Id, new DateTime(2025, 01, 01), new DateTime(2025, 01, 31));

        // Act
        var result = await _service.ExecuteAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var statement = result.Value!;
        statement.Items.Should().BeEquivalentTo(itemsInPeriod);
    }
}