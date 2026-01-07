using BankChu.CoreBanking.Application.Abstractions.Persistence;
using BankChu.CoreBanking.Application.Common.Errors;
using BankChu.CoreBanking.Application.Common.Results;
using BankChu.CoreBanking.Domain.Entities;
using FluentValidation;

namespace BankChu.CoreBanking.Application.Accounts.Create;

public sealed class CreateAccountService
{
    private readonly IAccountRepository accountRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly IValidator<CreateAccountCommand> validator;

    public CreateAccountService(
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork,
        IValidator<CreateAccountCommand> validator)
    {
        this.accountRepository = accountRepository;
        this.unitOfWork = unitOfWork;
        this.validator = validator;
    }

    public async Task<Result<CreateAccountResult>> ExecuteAsync(
        CreateAccountCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            return Result<CreateAccountResult>.Failure(AccountErrors.InvalidData);
        }

        var accountAlreadyExists = await accountRepository.ExistsByDocumentAsync(
                                            command.Document,
                                            cancellationToken);

        if (accountAlreadyExists)
        {
            return Result<CreateAccountResult>.Failure(AccountErrors.AlreadyExists);
        }

        var account = new Account(
            command.Document,
            command.Name,
            command.InitialBalance);

        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            await accountRepository.AddAsync(account, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);
        }
        catch
        {
            await unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }

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