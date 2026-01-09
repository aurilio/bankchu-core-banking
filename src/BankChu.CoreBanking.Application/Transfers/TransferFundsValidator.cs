using FluentValidation;

namespace BankChu.CoreBanking.Application.Transfers;

public sealed class TransferFundsValidator : AbstractValidator<TransferFundsCommand>
{
    public TransferFundsValidator()
    {
        RuleFor(x => x.FromAccountId)
            .NotEmpty();

        RuleFor(x => x.ToAccountId)
            .NotEmpty();

        RuleFor(x => x)
            .Must(x => x.FromAccountId != x.ToAccountId)
            .WithMessage("Source and destination accounts must be different.");

        RuleFor(x => x.Amount)
            .GreaterThan(0);

        RuleFor(x => x.IdempotencyKey)
            .NotEmpty()
            .MaximumLength(100);
    }
}