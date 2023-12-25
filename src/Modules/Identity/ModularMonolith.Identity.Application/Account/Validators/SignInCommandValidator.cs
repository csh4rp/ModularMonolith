using FluentValidation;
using ModularMonolith.Identity.Contracts.Account.Commands;

namespace ModularMonolith.Identity.Application.Account.Validators;

internal sealed class SignInCommandValidator : AbstractValidator<SignInCommand>
{
    public SignInCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .MaximumLength(128)
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty()
            .MaximumLength(128);
    }
}
