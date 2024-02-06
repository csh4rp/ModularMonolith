using FluentValidation;
using ModularMonolith.Identity.Contracts.Account.Registration;

namespace ModularMonolith.Identity.Application.Account.Registration;

internal sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .MaximumLength(128)
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(x => x.PasswordConfirmed)
            .NotEmpty()
            .MaximumLength(128)
            .Equal(x => x.Password);
    }
}
