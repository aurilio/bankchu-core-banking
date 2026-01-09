using BankChu.CoreBanking.Application.Abstractions.Persistence;
using BankChu.CoreBanking.Application.Abstractions.Services;
using BankChu.CoreBanking.Application.Accounts.Create;
using BankChu.CoreBanking.Application.Common.Erros;
using BankChu.CoreBanking.Application.Statements;
using BankChu.CoreBanking.Application.Transfers;
using BankChu.CoreBanking.Domain.Entities;
using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace BankChu.CoreBanking.Application.Tests.Transfers;

public sealed class TransferFundsServiceTests
{
    private readonly IAccountRepository _accountRepository;
    private readonly ITransferRepository _transferRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBusinessDayService _businessDayService;
    private readonly IIdempotencyService _idempotencyService;
    private readonly IValidator<TransferFundsCommand> _validator;
    private readonly ILogger<TransferFundsService> _logger;

    private readonly TransferFundsService _service;

    public TransferFundsServiceTests()
    {
        _accountRepository = Substitute.For<IAccountRepository>();
        _transferRepository = Substitute.For<ITransferRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _businessDayService = Substitute.For<IBusinessDayService>();
        _idempotencyService = Substitute.For<IIdempotencyService>();
        _validator = Substitute.For<IValidator<TransferFundsCommand>>();
        _logger = Substitute.For<ILogger<TransferFundsService>>();

        _idempotencyService
            .TryAcquireAsync(
                Arg.Any<string>(),
                Arg.Any<TimeSpan>(),
                Arg.Any<CancellationToken>())
            .Returns(true);

        _unitOfWork
            .ExecuteAsync(
                Arg.Any<Func<CancellationToken, Task<Transfer>>>(),
                Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var action = call.Arg<Func<CancellationToken, Task<Transfer>>>();
                return action(CancellationToken.None);
            });

        _service = new TransferFundsService(
            _accountRepository,
            _transferRepository,
            _unitOfWork,
            _businessDayService,
            _idempotencyService,
            _validator,
            _logger);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Return_Failure_When_Not_Business_Day()
    {
        // Arrange
        var command = CreateCommand();

        _businessDayService
            .IsBusinessDayAsync(command.RequestedDate, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _service.ExecuteAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be(TransferErrors.NotBusinessDay.Code);

        await _transferRepository.DidNotReceive()
            .AddAsync(Arg.Any<Transfer>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_Should_Create_Transfer_When_Valid()
    {
        // Arrange
        var command = CreateCommand();

        _businessDayService
            .IsBusinessDayAsync(command.RequestedDate, Arg.Any<CancellationToken>())
            .Returns(true);

        _accountRepository.GetByIdAsync(command.FromAccountId, Arg.Any<CancellationToken>())
            .Returns(CreateAccount(command.FromAccountId, true, 1000));

        _accountRepository.GetByIdAsync(command.ToAccountId, Arg.Any<CancellationToken>())
            .Returns(CreateAccount(command.ToAccountId, true, 100));

        // Act
        var result = await _service.ExecuteAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await _transferRepository.Received(1)
            .AddAsync(Arg.Any<Transfer>(), Arg.Any<CancellationToken>());

        await _idempotencyService.Received(1)
            .MarkCompletedAsync(
                command.IdempotencyKey,
                Arg.Any<TimeSpan>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_Should_Return_Conflict_When_Idempotency_Already_Exists()
    {
        // Arrange
        var command = CreateCommand();

        _idempotencyService
            .TryAcquireAsync(
                command.IdempotencyKey,
                Arg.Any<TimeSpan>(),
                Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _service.ExecuteAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be(TransferErrors.DuplicateRequestInProgress.Code);
    }

    private static TransferFundsCommand CreateCommand()
        => new(
            FromAccountId: Guid.NewGuid(),
            ToAccountId: Guid.NewGuid(),
            Amount: 100,
            RequestedDate: DateOnly.FromDateTime(DateTime.UtcNow),
            IdempotencyKey: Guid.NewGuid().ToString("N"));

    private static Account CreateAccount(Guid id, bool isActive, decimal balance)
    {
        var account = new Account("12345678901", "Test", 0);

        typeof(Account).GetProperty(nameof(Account.Id))!.SetValue(account, id);
        typeof(Account).GetProperty(nameof(Account.IsActive))!.SetValue(account, isActive);
        typeof(Account).GetProperty(nameof(Account.Balance))!.SetValue(account, balance);

        return account;
    }
}