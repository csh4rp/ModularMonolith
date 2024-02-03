using ModularMonolith.CategoryManagement.Contracts.Categories.Commands;
using ModularMonolith.CategoryManagement.Domain.Categories;
using ModularMonolith.Shared.Application.Commands;
using ModularMonolith.Shared.Contracts;
using ModularMonolith.Shared.Contracts.Errors;

namespace ModularMonolith.CategoryManagement.Application.Categories.CommandHandlers;

internal sealed class UpdateCategoryCommandHandler : ICommandHandler<UpdateCategoryCommand>
{
    private readonly ICategoryRepository _categoryRepository;

    public UpdateCategoryCommandHandler(ICategoryRepository categoryRepository) => _categoryRepository = categoryRepository;

    public async Task<Result> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.FindByIdAsync(new CategoryId(request.Id), cancellationToken);
        if (category is null)
        {
            return new EntityNotFoundError(nameof(Category), request.Id);
        }

        var categoryWithName = await _categoryRepository.FindByNameAsync(request.Name, cancellationToken);
        if (categoryWithName is not null && categoryWithName.Id != category.Id)
        {
            return new ConflictError(nameof(request.Name));
        }

        if (request.ParentId.HasValue)
        {
            var parentExists = await _categoryRepository.ExistsByNameAsync(request.Name, cancellationToken);
            if (!parentExists)
            {
                return MemberError.InvalidValue(nameof(UpdateCategoryCommand.ParentId));
            }
        }
        
        category.Update(request.ParentId.HasValue ? new CategoryId(request.ParentId.Value) : null,
            request.Name);

        await _categoryRepository.UpdateAsync(category, cancellationToken);

        return Result.Successful;
    }
}
