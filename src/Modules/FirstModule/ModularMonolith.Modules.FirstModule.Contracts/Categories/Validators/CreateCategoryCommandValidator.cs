using FluentValidation;
using ModularMonolith.Modules.FirstModule.Contracts.Categories.Commands;

namespace ModularMonolith.Modules.FirstModule.Contracts.Categories.Validators;

internal sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator() =>
        RuleFor(x => x.Name)
            .NotEmpty();
}
