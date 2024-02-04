using FluentValidation;
using ModularMonolith.CategoryManagement.Contracts.Categories.Creation;

namespace ModularMonolith.CategoryManagement.Application.Categories.Creation;

internal sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator() =>
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(128);
}
