
using BankChu.CoreBanking.Application.Abstractions.Persistence;
using BankChu.CoreBanking.Application.Common.Errors;
using BankChu.CoreBanking.Application.Common.Results;
using BankChu.CoreBanking.Application.Statements.Enum;
using Microsoft.Extensions.Logging;

namespace BankChu.CoreBanking.Application.Statements;

public sealed class GetStatementService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IStatementRepository _statementRepository;
    private readonly ILogger<GetStatementService> _logger;

    public GetStatementService(
        IAccountRepository accountRepository,
        IStatementRepository statementRepository,
        ILogger<GetStatementService> logger)
    {
        _accountRepository = accountRepository;
        _statementRepository = statementRepository;
        _logger = logger;
    }

    public async Task<Result<GetStatementResult>> ExecuteAsync(GetStatementQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Executing statement query. AccountId={AccountId}, From={From}, To={To}",
            query.AccountId,
            query.From,
            query.To);

        var account = await _accountRepository.GetByIdAsync(query.AccountId, cancellationToken);

        if (account is null)
        {
            _logger.LogWarning("Statement query failed. Account not found. AccountId={AccountId}", query.AccountId);

            return Result<GetStatementResult>.Failure(AccountErrors.NotFound(query.AccountId));
        }

        var items = await _statementRepository.GetAsync(
            query.AccountId,
            query.From,
            query.To,
            cancellationToken);

        _logger.LogDebug(
            "Statement items retrieved. AccountId={AccountId}, ItemsCount={ItemsCount}",
            query.AccountId,
            items.Count);

        var openingBalance = account.InitialBalance;
        var runningBalance = openingBalance;

        var orderedItems = items.OrderBy(i => i.OccurredAt).ToList();

        var statementLines = orderedItems.Select(item =>
        {
            runningBalance += item.Type == StatementType.Credit
                ? item.Amount
                : -item.Amount;

            return new StatementLine(
                item.TransferId,
                item.OccurredAt,
                item.Amount,
                item.Type,
                item.CounterpartyAccountId,
                runningBalance);
        }).ToList();

        var closingBalance = runningBalance;

        _logger.LogInformation(
            "Statement generated successfully. AccountId={AccountId}, OpeningBalance={OpeningBalance}, ClosingBalance={ClosingBalance}",
            query.AccountId,
            openingBalance,
            closingBalance);

        return Result<GetStatementResult>.Success(
            new GetStatementResult(
                query.AccountId,
                query.From,
                query.To,
                openingBalance,
                closingBalance,
                statementLines));
    }
}