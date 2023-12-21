using FluentValidation;
using ModularMonolith.Modules.FirstModule.Contracts.Categories.Queries;
using ModularMonolith.Shared.Contracts.Validators;

namespace ModularMonolith.Modules.FirstModule.Contracts.Categories.Validators;

internal sealed class FindCategoriesQueryValidator : PaginatedQueryValidator<FindCategoriesQuery>
{
    public FindCategoriesQueryValidator()
    {
        RuleFor(x => x.Search)
            .MaximumLength(128);
    }
}
