using FluentValidation;
using ModularMonolith.FirstModule.Contracts.Categories.Queries;
using ModularMonolith.Shared.Contracts.Validators;

namespace ModularMonolith.FirstModule.Contracts.Categories.Validators;

internal sealed class FindCategoriesQueryValidator : PaginatedQueryValidator<FindCategoriesQuery>
{
    public FindCategoriesQueryValidator() =>
        RuleFor(x => x.Search)
            .MaximumLength(128);
}
