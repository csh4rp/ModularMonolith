using FluentValidation;
using ModularMonolith.Identity.Contracts.Account.PasswordChange;

namespace ModularMonolith.Identity.Application.Account.PasswordChange;

internal sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(x => x.NewPasswordConfirmed)
            .NotEmpty()
            .MaximumLength(128)
            .Equal(x => x.NewPassword);
    }
}
