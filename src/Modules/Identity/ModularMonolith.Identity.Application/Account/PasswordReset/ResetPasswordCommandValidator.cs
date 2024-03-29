﻿using FluentValidation;
using ModularMonolith.Identity.Contracts.Account.PasswordReset;

namespace ModularMonolith.Identity.Application.Account.PasswordReset;

internal sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.ResetPasswordToken)
            .NotEmpty()
            .MaximumLength(1024);

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(x => x.NewPasswordConfirmed)
            .NotEmpty()
            .MaximumLength(128)
            .Equal(x => x.NewPassword);
    }
}
