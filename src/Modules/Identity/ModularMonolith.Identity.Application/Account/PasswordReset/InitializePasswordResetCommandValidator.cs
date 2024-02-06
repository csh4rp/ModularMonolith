using FluentValidation;
using ModularMonolith.Identity.Contracts.Account.PasswordReset;

namespace ModularMonolith.Identity.Application.Account.PasswordReset;

internal sealed class InitializePasswordResetCommandValidator : AbstractValidator<InitializePasswordResetCommand>
{
    public InitializePasswordResetCommandValidator() =>
        RuleFor(x => x.Email)
            .NotEmpty()
            .MaximumLength(128)
            .EmailAddress();
}
