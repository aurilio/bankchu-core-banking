
using BankChu.CoreBanking.Application.Abstractions.Persistence;
using BankChu.CoreBanking.Application.Common.Errors;
using BankChu.CoreBanking.Application.Common.Results;
using BankChu.CoreBanking.Application.Statements.Dto;
using BankChu.CoreBanking.Application.Statements.Enum;

namespace BankChu.CoreBanking.Application.Statements;

public sealed class GetStatementService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IStatementRepository _statementRepository;

    public GetStatementService(
        IAccountRepository accountRepository,
        IStatementRepository statementRepository)
    {
        _accountRepository = accountRepository;
        _statementRepository = statementRepository;
    }

    public async Task<Result<GetStatementResult>> ExecuteAsync(
    GetStatementQuery query,
    CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(query.AccountId, cancellationToken);

        if (account is null)
            return Result<GetStatementResult>.Failure(AccountErrors.NotFound(query.AccountId));

        var currentBalance = account.Balance;

        var afterPeriod = await _statementRepository.GetAfterAsync(
            query.AccountId,
            query.To,
            cancellationToken);

        var closingBalance = afterPeriod.Aggregate(
            currentBalance,
            (balance, item) =>
                item.Type == StatementType.Credit
                    ? balance - item.Amount
                    : balance + item.Amount);

        var periodItems = await _statementRepository.GetAsync(
            query.AccountId,
            query.From,
            query.To,
            cancellationToken);

        var orderedItems = periodItems
            .OrderBy(x => x.OccurredAt)
            .ThenBy(x => x.TransferId)
            .ToList();

        var openingBalance = orderedItems
            .Reverse<StatementItemDto>()
            .Aggregate(
                closingBalance,
                (balance, item) =>
                    item.Type == StatementType.Credit
                        ? balance - item.Amount   // desfaz crédito
                        : balance + item.Amount); // desfaz débito

        var runningBalance = openingBalance;

        var materializedItems = orderedItems
            .Select(item =>
            {
                runningBalance += item.Type == StatementType.Credit
                    ? item.Amount
                    : -item.Amount;

                return new StatementItem(
                    item.TransferId,
                    item.OccurredAt,
                    item.Amount,
                    item.Type,
                    item.CounterpartyAccountId,
                    runningBalance);
            })
            .ToList();

        return Result<GetStatementResult>.Success(
            new GetStatementResult(
                query.AccountId,
                query.From,
                query.To,
                openingBalance,
                closingBalance,
                materializedItems));
    }
}