using FluentValidation;
using ModularMonolith.CategoryManagement.Contracts.Categories.Queries;
using ModularMonolith.Shared.Contracts.Validators;

namespace ModularMonolith.CategoryManagement.Application.Categories.Validators;

internal sealed class FindCategoriesQueryValidator : PaginatedQueryValidator<FindCategoriesQuery>
{
    public FindCategoriesQueryValidator() =>
        RuleFor(x => x.Search)
            .MaximumLength(128);
}
