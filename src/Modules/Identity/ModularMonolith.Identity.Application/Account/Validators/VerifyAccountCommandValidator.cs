using FluentValidation;
using ModularMonolith.Identity.Contracts.Account.Commands;

namespace ModularMonolith.Identity.Application.Account.Validators;

internal sealed class VerifyAccountCommandValidator : AbstractValidator<VerifyAccountCommand>
{
    public VerifyAccountCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();
        
        RuleFor(x => x.VerificationToken)
            .NotEmpty()
            .MaximumLength(1024);
    }
}
