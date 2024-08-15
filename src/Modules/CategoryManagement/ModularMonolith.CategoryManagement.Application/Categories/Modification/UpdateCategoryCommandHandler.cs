using ModularMonolith.CategoryManagement.Application.Categories.Shared;
using ModularMonolith.CategoryManagement.Contracts.Categories.Creation;
using ModularMonolith.CategoryManagement.Contracts.Categories.Modification;
using ModularMonolith.CategoryManagement.Domain.Categories;
using ModularMonolith.Shared.Application.Commands;
using ModularMonolith.Shared.Application.Exceptions;
using ModularMonolith.Shared.Contracts.Errors;

namespace ModularMonolith.CategoryManagement.Application.Categories.Modification;

internal sealed class UpdateCategoryCommandHandler : ICommandHandler<UpdateCategoryCommand>
{
    private readonly ICategoryRepository _categoryRepository;

    public UpdateCategoryCommandHandler(ICategoryRepository categoryRepository) =>
        _categoryRepository = categoryRepository;

    public async Task Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.FindByIdAsync(CategoryId.From(request.Id), cancellationToken)
                       ?? throw new EntityNotFoundException(typeof(Category), request.Id);

        var categoryWithName = await _categoryRepository.FindByNameAsync(request.Name, cancellationToken);
        if (categoryWithName is not null && categoryWithName.Id != category.Id)
        {
            throw new CategoryNameConflictException("Category with given name already exists", nameof(request.Name));
        }

        if (request.ParentId.HasValue)
        {
            var parentExists = await _categoryRepository.ExistsByNameAsync(request.Name, cancellationToken);
            if (!parentExists)
            {
                throw new ValidationException(MemberError.InvalidValue(nameof(CreateCategoryCommand.ParentId)));
            }
        }

        category.Update(request.ParentId.HasValue ? CategoryId.From(request.ParentId.Value) : null,
            request.Name);

        await _categoryRepository.UpdateAsync(category, cancellationToken);
    }
}
