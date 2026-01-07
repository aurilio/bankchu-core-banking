using FluentValidation;

namespace BankChu.CoreBanking.Application.Accounts.Create;

public sealed class CreateAccountValidator
    : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountValidator()
    {
        RuleFor(x => x.Document)
            .NotEmpty()
            .Length(11, 14);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.InitialBalance)
            .GreaterThanOrEqualTo(0);
    }
}