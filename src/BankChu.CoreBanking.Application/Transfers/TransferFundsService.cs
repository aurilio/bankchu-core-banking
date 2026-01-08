using BankChu.CoreBanking.Application.Abstractions.Persistence;
using BankChu.CoreBanking.Application.Abstractions.Services;
using BankChu.CoreBanking.Application.Common.Erros;
using BankChu.CoreBanking.Application.Common.Results;
using BankChu.CoreBanking.Domain.Entities;

namespace BankChu.CoreBanking.Application.Transfers;

public sealed class TransferFundsService
{
    private static readonly TimeSpan IdempotencyInProgressTtl = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan IdempotencyCompletedTtl = TimeSpan.FromHours(24);

    private readonly IAccountRepository _accountRepository;
    private readonly ITransferRepository _transferRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBusinessDayService _businessDayService;
    private readonly IIdempotencyService _idempotencyService;

    public TransferFundsService(
        IAccountRepository accountRepository,
        ITransferRepository transferRepository,
        IUnitOfWork unitOfWork,
        IBusinessDayService businessDayService,
        IIdempotencyService idempotencyService)
    {
        _accountRepository = accountRepository;
        _transferRepository = transferRepository;
        _unitOfWork = unitOfWork;
        _businessDayService = businessDayService;
        _idempotencyService = idempotencyService;
    }

    public async Task<Result<TransferFundsResult>> ExecuteAsync(
        TransferFundsCommand command,
        CancellationToken cancellationToken)
    {
        var acquired = await _idempotencyService.TryAcquireAsync(command.IdempotencyKey, IdempotencyInProgressTtl, cancellationToken);

        if (!acquired)
            return Result<TransferFundsResult>.Failure(TransferErrors.DuplicateRequestInProgress);

        Result<TransferFundsResult> result;

        try
        {
            result = await ExecuteCoreAsync(command, cancellationToken);
        }
        catch
        {
            throw;
        }

        if (result.IsSuccess)
        {
            await _idempotencyService.MarkCompletedAsync(command.IdempotencyKey, IdempotencyCompletedTtl, cancellationToken);
        }

        return result;
    }

    private async Task<Result<TransferFundsResult>> ExecuteCoreAsync(
        TransferFundsCommand command,
        CancellationToken cancellationToken)
    {
        if (command.Amount <= 0)
            return Result<TransferFundsResult>.Failure(TransferErrors.InvalidAmount);

        if (command.FromAccountId == command.ToAccountId)
            return Result<TransferFundsResult>.Failure(TransferErrors.SameAccount);

        var isBusinessDay = await _businessDayService.IsBusinessDayAsync(command.RequestedDate, cancellationToken);

        if (!isBusinessDay)
            return Result<TransferFundsResult>.Failure(TransferErrors.NotBusinessDay);

        var fromAccount = await _accountRepository.GetByIdAsync(command.FromAccountId, cancellationToken);

        if (fromAccount is null)
            return Result<TransferFundsResult>.Failure(TransferErrors.SourceAccountNotFound);

        var toAccount = await _accountRepository.GetByIdAsync(command.ToAccountId, cancellationToken);

        if (toAccount is null)
            return Result<TransferFundsResult>.Failure(TransferErrors.DestinationAccountNotFound);

        if (!fromAccount.IsActive)
            return Result<TransferFundsResult>.Failure(TransferErrors.SourceAccountInactive);

        if (!toAccount.IsActive)
            return Result<TransferFundsResult>.Failure(TransferErrors.DestinationAccountInactive);

        if (fromAccount.Balance < command.Amount)
            return Result<TransferFundsResult>.Failure(TransferErrors.InsufficientBalance);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            fromAccount.Debit(command.Amount);
            toAccount.Credit(command.Amount);

            var transfer = new Transfer(
                command.FromAccountId,
                command.ToAccountId,
                command.Amount);

            await _transferRepository.AddAsync(transfer, cancellationToken);

            await _unitOfWork.CommitAsync(cancellationToken);

            return Result<TransferFundsResult>.Success(
                new TransferFundsResult(
                    transfer.Id,
                    transfer.FromAccountId,
                    transfer.ToAccountId,
                    transfer.Amount,
                    transfer.CreatedAt,
                    transfer.Status));
        }
        catch
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }
}