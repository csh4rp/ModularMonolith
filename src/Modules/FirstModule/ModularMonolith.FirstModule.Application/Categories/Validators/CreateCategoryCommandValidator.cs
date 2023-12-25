using FluentValidation;
using ModularMonolith.FirstModule.Contracts.Categories.Commands;

namespace ModularMonolith.FirstModule.Application.Categories.Validators;

internal sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator() =>
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(128);
}
