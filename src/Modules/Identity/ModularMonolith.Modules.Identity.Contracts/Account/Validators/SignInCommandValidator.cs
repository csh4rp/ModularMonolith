using FluentValidation;
using ModularMonolith.Modules.Identity.Contracts.Account.Commands;

namespace ModularMonolith.Modules.Identity.Contracts.Account.Validators;

public class SignInCommandValidator : AbstractValidator<SignInCommand>
{
    public SignInCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty()
            .MaximumLength(128);
    }
}
