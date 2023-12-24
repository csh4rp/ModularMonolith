using FluentValidation;
using ModularMonolith.FirstModule.Contracts.Categories.Commands;

namespace ModularMonolith.FirstModule.Contracts.Categories.Validators;

internal sealed class DeleteCategoryCommandValidator : AbstractValidator<DeleteCategoryCommand>
{
}
