using BankChu.CoreBanking.Application.Abstractions.Persistence;
using BankChu.CoreBanking.Application.Accounts.Create;
using BankChu.CoreBanking.Application.Common.Results;
using BankChu.CoreBanking.Application.Statements;
using BankChu.CoreBanking.Domain.Entities;
using Bogus;
using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace BankChu.CoreBanking.Application.Tests.Accounts.Create;

public sealed class CreateAccountServiceTests
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateAccountCommand> _validator;
    private readonly ILogger<CreateAccountService> _logger;
    private readonly CreateAccountService _service;
    private readonly Faker _faker;

    public CreateAccountServiceTests()
    {
        _accountRepository = Substitute.For<IAccountRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _validator = Substitute.For<IValidator<CreateAccountCommand>>();
        _logger = Substitute.For<ILogger<CreateAccountService>>();

        _service = new CreateAccountService(
            _accountRepository,
            _unitOfWork,
            _validator,
            _logger);

        _faker = new Faker();

        _unitOfWork
            .ExecuteAsync(
                Arg.Any<Func<CancellationToken, Task>>(),
                Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var action = call.Arg<Func<CancellationToken, Task>>();
                return action(CancellationToken.None);
            });
        _validator
            .ValidateAsync(
                Arg.Any<CreateAccountCommand>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(
                new FluentValidation.Results.ValidationResult()));
    }

    [Fact]
    public async Task ExecuteAsync_Should_Return_Failure_When_Account_Already_Exists()
    {
        // Arrange
        var command = CreateValidCommand();

        _accountRepository
            .ExistsByDocumentAsync(command.Document, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _service.ExecuteAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Type.Should().Be(ErrorType.Conflict);
        result.Error.Code.Should().Be("ACCOUNT_ALREADY_EXISTS");
    }

    [Fact]
    public async Task ExecuteAsync_Should_Create_Account_When_Command_Is_Valid()
    {
        // Arrange
        var command = CreateValidCommand();

        _accountRepository
            .ExistsByDocumentAsync(command.Document, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _service.ExecuteAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        var account = result.Value!;

        account.Id.Should().NotBeEmpty();
        account.Document.Should().Be(command.Document);
        account.Name.Should().Be(command.Name);
        account.Balance.Should().Be(command.InitialBalance);
        account.IsActive.Should().BeTrue();

        await _accountRepository.Received(1)
            .AddAsync(
                Arg.Is<Account>(a =>
                    a.Document == command.Document &&
                    a.Name == command.Name &&
                    a.Balance == command.InitialBalance),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_Should_Rollback_When_Persistence_Fails()
    {
        // Arrange
        var command = CreateValidCommand();

        _accountRepository
            .ExistsByDocumentAsync(command.Document, Arg.Any<CancellationToken>())
            .Returns(false);

        _accountRepository
            .AddAsync(Arg.Any<Account>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new Exception("Database failure")));

        // Act
        var act = async () => await _service.ExecuteAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        
        await _accountRepository.Received(1)
                    .AddAsync(Arg.Any<Account>(), Arg.Any<CancellationToken>());
    }

    private CreateAccountCommand CreateValidCommand()
    {
        return new CreateAccountCommand(
            Document: _faker.Random.ReplaceNumbers("###########"),
            Name: _faker.Person.FullName,
            InitialBalance: _faker.Random.Decimal(0, 10_000));
    }
}