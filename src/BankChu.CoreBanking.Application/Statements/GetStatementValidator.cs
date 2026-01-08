using FluentValidation;

namespace BankChu.CoreBanking.Application.Statements;

public sealed class GetStatementValidator : AbstractValidator<GetStatementQuery>
{
    public GetStatementValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();

        RuleFor(x => x.From)
            .LessThanOrEqualTo(x => x.To);

        RuleFor(x => x)
            .Must(x => (x.To - x.From).TotalDays <= 90)
            .WithMessage("The maximum allowed period is 90 days.");
    }
}