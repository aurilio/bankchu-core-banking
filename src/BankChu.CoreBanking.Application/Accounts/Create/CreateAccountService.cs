using BankChu.CoreBanking.Application.Abstractions.Persistence;
using BankChu.CoreBanking.Application.Common.Errors;
using BankChu.CoreBanking.Application.Common.Results;
using BankChu.CoreBanking.Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace BankChu.CoreBanking.Application.Accounts.Create;

public sealed class CreateAccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateAccountCommand> _validator;
    private readonly ILogger<CreateAccountService> _logger;

    public CreateAccountService(
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork,
        IValidator<CreateAccountCommand> validator,
        ILogger<CreateAccountService> logger)
    {
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<CreateAccountResult>> ExecuteAsync(CreateAccountCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Starting account creation. DocumentHash={DocumentHash}, InitialBalance={InitialBalance}",
            command.Document.GetHashCode(),
            command.InitialBalance);

        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            return Result<CreateAccountResult>.Failure(AccountErrors.InvalidData);
        }

        var accountAlreadyExists = await _accountRepository.ExistsByDocumentAsync(command.Document, cancellationToken);

        if (accountAlreadyExists)
        {
            _logger.LogWarning("Account creation failed. Account already exists. DocumentHash={DocumentHash}", command.Document.GetHashCode());

            return Result<CreateAccountResult>.Failure(AccountErrors.AlreadyExists);
        }

        var account = new Account(
            command.Document,
            command.Name,
            command.InitialBalance);

        await _unitOfWork.ExecuteAsync(async ct =>
        {
            await _accountRepository.AddAsync(account, ct);
        }, cancellationToken);

        _logger.LogInformation(
            "Account created successfully. AccountId={AccountId}, InitialBalance={InitialBalance}",
            account.Id,
            account.InitialBalance);

        var result = new CreateAccountResult(
            account.Id,
            account.Document,
            account.Name,
            account.Balance,
            account.IsActive,
            account.CreatedAt);

        return Result<CreateAccountResult>.Success(result);
    }
}