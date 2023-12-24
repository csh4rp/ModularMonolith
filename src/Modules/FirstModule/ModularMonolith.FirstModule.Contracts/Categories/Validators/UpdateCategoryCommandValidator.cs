using FluentValidation;
using ModularMonolith.FirstModule.Contracts.Categories.Commands;

namespace ModularMonolith.FirstModule.Contracts.Categories.Validators;

internal sealed class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator() =>
        RuleFor(x => x.Name)
            .NotEmpty();
}
