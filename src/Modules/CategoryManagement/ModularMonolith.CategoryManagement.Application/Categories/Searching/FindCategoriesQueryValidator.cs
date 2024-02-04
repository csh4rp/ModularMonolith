using FluentValidation;
using ModularMonolith.CategoryManagement.Contracts.Categories.Searching;
using ModularMonolith.Shared.Contracts.Validators;

namespace ModularMonolith.CategoryManagement.Application.Categories.Searching;

internal sealed class FindCategoriesQueryValidator : PaginatedQueryValidator<FindCategoriesQuery>
{
    public FindCategoriesQueryValidator() =>
        RuleFor(x => x.Search)
            .MaximumLength(128);
}
