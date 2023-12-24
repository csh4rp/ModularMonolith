using FluentValidation;
using ModularMonolith.FirstModule.Contracts.Categories.Commands;

namespace ModularMonolith.FirstModule.Contracts.Categories.Validators;

internal sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator() =>
        RuleFor(x => x.Name)
            .NotEmpty();
}
