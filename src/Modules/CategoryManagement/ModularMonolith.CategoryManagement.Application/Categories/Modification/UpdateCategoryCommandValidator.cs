using FluentValidation;
using ModularMonolith.CategoryManagement.Contracts.Categories.Modification;

namespace ModularMonolith.CategoryManagement.Application.Categories.Modification;

internal sealed class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator() =>
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(128);
}
