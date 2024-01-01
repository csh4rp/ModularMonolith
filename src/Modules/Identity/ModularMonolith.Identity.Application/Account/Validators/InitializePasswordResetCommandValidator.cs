using FluentValidation;
using ModularMonolith.Identity.Contracts.Account.Commands;

namespace ModularMonolith.Identity.Application.Account.Validators;

internal sealed class InitializePasswordResetCommandValidator : AbstractValidator<InitializePasswordResetCommand>
{
    public InitializePasswordResetCommandValidator() =>
        RuleFor(x => x.Email)
            .NotEmpty()
            .MaximumLength(128)
            .EmailAddress();
}
