using FluentValidation;

namespace ModularMonolith.Shared.Contracts.Validators;

public abstract class PaginatedQueryValidator<T> : AbstractValidator<T> where T : IPaginatedQuery
{
    protected PaginatedQueryValidator()
    {
        RuleFor(x => x.OrderBy)
            .MaximumLength(128);

        RuleFor(x => x.Skip)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Take)
            .GreaterThanOrEqualTo(1);
    }
}
