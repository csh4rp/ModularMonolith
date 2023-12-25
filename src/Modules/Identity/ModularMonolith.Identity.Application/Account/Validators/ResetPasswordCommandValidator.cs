using FluentValidation;
using ModularMonolith.Identity.Contracts.Account.Commands;

namespace ModularMonolith.Identity.Application.Account.Validators;

internal sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();
        
        RuleFor(x => x.ResetPasswordToken)
            .NotEmpty()
            .MaximumLength(256);
        
        RuleFor(x => x.Password)
            .NotEmpty()
            .MaximumLength(128);
        
        RuleFor(x => x.PasswordConfirmed)
            .NotEmpty()
            .MaximumLength(128)
            .Equal(x => x.Password);
    }
}
