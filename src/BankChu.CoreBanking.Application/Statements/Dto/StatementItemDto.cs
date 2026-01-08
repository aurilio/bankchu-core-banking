using BankChu.CoreBanking.Application.Statements.Enum;

namespace BankChu.CoreBanking.Application.Statements.Dto;

public sealed record StatementItemDto(
    Guid TransferId,
    DateTime OccurredAt,
    decimal Amount,
    StatementType Type,
    Guid CounterpartyAccountId
);