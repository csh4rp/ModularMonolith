using FluentValidation;
using ModularMonolith.Identity.Contracts.Account.SigningIn;

namespace ModularMonolith.Identity.Application.Account.SigningIn;

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
