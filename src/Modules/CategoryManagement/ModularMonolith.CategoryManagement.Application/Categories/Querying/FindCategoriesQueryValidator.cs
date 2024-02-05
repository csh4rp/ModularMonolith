using FluentValidation;
using ModularMonolith.CategoryManagement.Contracts.Categories.Querying;
using ModularMonolith.Shared.Contracts.Validators;

namespace ModularMonolith.CategoryManagement.Application.Categories.Querying;

internal sealed class FindCategoriesQueryValidator : PaginatedQueryValidator<FindCategoriesQuery>
{
    public FindCategoriesQueryValidator() =>
        RuleFor(x => x.SearchTerm)
            .MaximumLength(128);
}
