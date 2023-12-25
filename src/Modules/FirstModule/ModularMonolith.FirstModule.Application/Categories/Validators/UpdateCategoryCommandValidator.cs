using FluentValidation;
using ModularMonolith.FirstModule.Contracts.Categories.Commands;

namespace ModularMonolith.FirstModule.Application.Categories.Validators;

internal sealed class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator() =>
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(128);
}
