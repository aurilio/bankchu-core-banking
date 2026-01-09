using BankChu.CoreBanking.Application.Abstractions.Persistence;
using BankChu.CoreBanking.Application.Abstractions.Services;
using BankChu.CoreBanking.Application.Common.Erros;
using BankChu.CoreBanking.Application.Common.Results;
using BankChu.CoreBanking.Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

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
    private readonly IValidator<TransferFundsCommand> _validator;
    private readonly ILogger<TransferFundsService> _logger;

    public TransferFundsService(
        IAccountRepository accountRepository,
        ITransferRepository transferRepository,
        IUnitOfWork unitOfWork,
        IBusinessDayService businessDayService,
        IIdempotencyService idempotencyService,
        IValidator<TransferFundsCommand> validator,
        ILogger<TransferFundsService> logger)
    {
        _accountRepository = accountRepository;
        _transferRepository = transferRepository;
        _unitOfWork = unitOfWork;
        _businessDayService = businessDayService;
        _idempotencyService = idempotencyService;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<TransferFundsResult>> ExecuteAsync(TransferFundsCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Starting transfer execution. FromAccount={FromAccountId}, ToAccount={ToAccountId}, Amount={Amount}, IdempotencyKey={IdempotencyKey}",
            command.FromAccountId,
            command.ToAccountId,
            command.Amount,
            command.IdempotencyKey);

        var validation = await _validator.ValidateAsync(command, cancellationToken);

        if (!validation.IsValid)
        {
            var details = validation.Errors.Select(e => e.ErrorMessage).ToArray();

            _logger.LogWarning("Transfer validation failed. Errors={Errors}", details);

            return Result<TransferFundsResult>.Failure(TransferErrors.ValidationFailed.WithDetails(details));
        }

        var acquired = await _idempotencyService.TryAcquireAsync(
                                                    command.IdempotencyKey,
                                                    IdempotencyInProgressTtl,
                                                    cancellationToken);

        if (!acquired)
        {
            _logger.LogWarning("Duplicate transfer request detected. IdempotencyKey={IdempotencyKey}", command.IdempotencyKey);

            return Result<TransferFundsResult>.Failure(TransferErrors.DuplicateRequestInProgress);
        }

        try
        {
            var result = await ExecuteCoreAsync(command, cancellationToken);

            if (result.IsSuccess)
            {
                await _idempotencyService.MarkCompletedAsync(
                                            command.IdempotencyKey,
                                            IdempotencyCompletedTtl,
                                            cancellationToken);

                _logger.LogInformation(
                    "Transfer completed successfully. TransferId={TransferId}, FromAccount={FromAccountId}, ToAccount={ToAccountId}, Amount={Amount}",
                    result.Value!.TransferId,
                    result.Value.FromAccountId,
                    result.Value.ToAccountId,
                    result.Value.Amount);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error while processing transfer. FromAccount={FromAccountId}, ToAccount={ToAccountId}, Amount={Amount}, IdempotencyKey={IdempotencyKey}",
                command.FromAccountId,
                command.ToAccountId,
                command.Amount,
                command.IdempotencyKey);

            throw;
        }
    }

    private async Task<Result<TransferFundsResult>> ExecuteCoreAsync(TransferFundsCommand command, CancellationToken cancellationToken)
    {
        if (command.Amount <= 0)
            return Result<TransferFundsResult>.Failure(TransferErrors.InvalidAmount);

        if (command.FromAccountId == command.ToAccountId)
            return Result<TransferFundsResult>.Failure(TransferErrors.SameAccount);

        var isBusinessDay = await _businessDayService
            .IsBusinessDayAsync(command.RequestedDate, cancellationToken);

        if (!isBusinessDay)
            return Result<TransferFundsResult>.Failure(TransferErrors.NotBusinessDay);

        var fromAccount = await _accountRepository
            .GetByIdAsync(command.FromAccountId, cancellationToken);

        if (fromAccount is null)
            return Result<TransferFundsResult>.Failure(TransferErrors.SourceAccountNotFound);

        var toAccount = await _accountRepository
            .GetByIdAsync(command.ToAccountId, cancellationToken);

        if (toAccount is null)
            return Result<TransferFundsResult>.Failure(TransferErrors.DestinationAccountNotFound);

        if (!fromAccount.IsActive)
            return Result<TransferFundsResult>.Failure(TransferErrors.SourceAccountInactive);

        if (!toAccount.IsActive)
            return Result<TransferFundsResult>.Failure(TransferErrors.DestinationAccountInactive);

        if (fromAccount.Balance < command.Amount)
            return Result<TransferFundsResult>.Failure(TransferErrors.InsufficientBalance);

        _logger.LogDebug(
            "Executing transfer transaction. FromAccount={FromAccountId}, ToAccount={ToAccountId}, Amount={Amount}",
            command.FromAccountId,
            command.ToAccountId,
            command.Amount);

        var transfer = await _unitOfWork.ExecuteAsync(async ct =>
        {
            fromAccount.Debit(command.Amount);
            toAccount.Credit(command.Amount);

            var transfer = new Transfer(
                command.FromAccountId,
                command.ToAccountId,
                command.Amount);

            await _transferRepository.AddAsync(transfer, ct);

            return transfer;
        }, cancellationToken);

        return Result<TransferFundsResult>.Success(
            new TransferFundsResult(
                transfer.Id,
                transfer.FromAccountId,
                transfer.ToAccountId,
                transfer.Amount,
                transfer.CreatedAt,
                transfer.Status));
    }
}