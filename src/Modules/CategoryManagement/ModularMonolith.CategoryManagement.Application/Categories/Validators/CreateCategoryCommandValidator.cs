using FluentValidation;
using ModularMonolith.CategoryManagement.Contracts.Categories.Commands;

namespace ModularMonolith.CategoryManagement.Application.Categories.Validators;

internal sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator() =>
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(128);
}
