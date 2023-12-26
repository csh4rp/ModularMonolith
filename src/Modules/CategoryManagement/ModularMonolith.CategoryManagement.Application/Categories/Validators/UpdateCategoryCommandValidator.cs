using FluentValidation;
using ModularMonolith.CategoryManagement.Contracts.Categories.Commands;

namespace ModularMonolith.CategoryManagement.Application.Categories.Validators;

internal sealed class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator() =>
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(128);
}
