using ModularMonolith.CategoryManagement.Application.Categories.Exceptions;
using ModularMonolith.CategoryManagement.Contracts.Categories.Commands;
using ModularMonolith.CategoryManagement.Domain.Categories;
using ModularMonolith.Shared.Application.Commands;
using ModularMonolith.Shared.Application.Exceptions;
using ModularMonolith.Shared.Contracts;
using ModularMonolith.Shared.Contracts.Errors;

namespace ModularMonolith.CategoryManagement.Application.Categories.CommandHandlers;

internal sealed class CreateCategoryCommandHandler : ICommandHandler<CreateCategoryCommand, CreatedResponse>
{
    private readonly ICategoryRepository _categoryRepository;

    public CreateCategoryCommandHandler(ICategoryRepository categoryRepository) => _categoryRepository = categoryRepository;

    public async Task<CreatedResponse> Handle(CreateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var categoryWithNameExists = await _categoryRepository.ExistsByNameAsync(request.Name, cancellationToken);
        if (categoryWithNameExists)
        {
            throw new CategoryNameConflictException("Category with given name already exists", nameof(request.Name));
        }

        if (request.ParentId.HasValue)
        {
            var parentExists = await _categoryRepository.ExistsByIdAsync(new CategoryId(request.ParentId.Value), cancellationToken);
            if (!parentExists)
            {
                throw new ValidationException(MemberError.InvalidValue(nameof(CreateCategoryCommand.ParentId)));
            }
        }

        var category = Category.Create(request.Name,
            request.ParentId.HasValue ? new CategoryId(request.ParentId.Value) : null);

        await _categoryRepository.AddAsync(category, cancellationToken);

        return new CreatedResponse(category.Id.Value);
    }
}
