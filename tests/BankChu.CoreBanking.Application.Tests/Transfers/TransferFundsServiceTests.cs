using BankChu.CoreBanking.Application.Abstractions.Persistence;
using BankChu.CoreBanking.Application.Abstractions.Services;
using BankChu.CoreBanking.Application.Common.Erros;
using BankChu.CoreBanking.Application.Transfers;
using BankChu.CoreBanking.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace BankChu.CoreBanking.Application.Tests.Transfers;

public sealed class TransferFundsServiceTests
{
    private readonly IAccountRepository _accountRepository;
    private readonly ITransferRepository _transferRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBusinessDayService _businessDayService;
    private readonly IIdempotencyService _idempotencyService;

    private readonly TransferFundsService _service;

    public TransferFundsServiceTests()
    {
        _accountRepository = Substitute.For<IAccountRepository>();
        _transferRepository = Substitute.For<ITransferRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _businessDayService = Substitute.For<IBusinessDayService>();
        _idempotencyService = Substitute.For<IIdempotencyService>();

        _idempotencyService
            .TryAcquireAsync(
                Arg.Any<string>(),
                Arg.Any<TimeSpan>(),
                Arg.Any<CancellationToken>())
            .Returns(true);

        _service = new TransferFundsService(
            _accountRepository,
            _transferRepository,
            _unitOfWork,
            _businessDayService,
            _idempotencyService);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Return_Failure_When_Not_Business_Day()
    {
        var command = CreateCommand();

        _businessDayService
            .IsBusinessDayAsync(command.RequestedDate, Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await _service.ExecuteAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be(TransferErrors.NotBusinessDay.Code);

        await _unitOfWork.DidNotReceive()
            .BeginTransactionAsync(Arg.Any<CancellationToken>());

        await _idempotencyService.DidNotReceive()
            .MarkCompletedAsync(Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_Should_Create_Transfer_When_Valid()
    {
        var command = CreateCommand();

        _businessDayService
            .IsBusinessDayAsync(command.RequestedDate, Arg.Any<CancellationToken>())
            .Returns(true);

        var from = CreateAccount(command.FromAccountId, true, 1000);
        var to = CreateAccount(command.ToAccountId, true, 100);

        _accountRepository.GetByIdAsync(command.FromAccountId, Arg.Any<CancellationToken>())
            .Returns(from);

        _accountRepository.GetByIdAsync(command.ToAccountId, Arg.Any<CancellationToken>())
            .Returns(to);

        var result = await _service.ExecuteAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        await _unitOfWork.Received(1)
            .BeginTransactionAsync(Arg.Any<CancellationToken>());

        await _transferRepository.Received(1)
            .AddAsync(Arg.Any<Transfer>(), Arg.Any<CancellationToken>());

        await _unitOfWork.Received(1)
            .CommitAsync(Arg.Any<CancellationToken>());

        await _idempotencyService.Received(1)
            .MarkCompletedAsync(
                command.IdempotencyKey,
                Arg.Any<TimeSpan>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_Should_Return_Conflict_When_Idempotency_Already_Exists()
    {
        var command = CreateCommand();

        _idempotencyService
            .TryAcquireAsync(
                command.IdempotencyKey,
                Arg.Any<TimeSpan>(),
                Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await _service.ExecuteAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be(
            TransferErrors.DuplicateRequestInProgress.Code);

        await _unitOfWork.DidNotReceive()
            .BeginTransactionAsync(Arg.Any<CancellationToken>());
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

        typeof(Account).GetProperty(nameof(Account.Id))!
            .SetValue(account, id);

        typeof(Account).GetProperty(nameof(Account.IsActive))!
            .SetValue(account, isActive);

        typeof(Account).GetProperty(nameof(Account.Balance))!
            .SetValue(account, balance);

        return account;
    }
}