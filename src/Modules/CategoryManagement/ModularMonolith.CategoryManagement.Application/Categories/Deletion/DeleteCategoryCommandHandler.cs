using ModularMonolith.CategoryManagement.Contracts.Categories.Deletion;
using ModularMonolith.CategoryManagement.Domain.Categories;
using ModularMonolith.Shared.Application.Commands;
using ModularMonolith.Shared.Application.Exceptions;

namespace ModularMonolith.CategoryManagement.Application.Categories.Deletion;

internal sealed class DeleteCategoryCommandHandler : ICommandHandler<DeleteCategoryCommand>
{
    private readonly ICategoryRepository _categoryRepository;

    public DeleteCategoryCommandHandler(ICategoryRepository categoryRepository) =>
        _categoryRepository = categoryRepository;

    public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.FindByIdAsync(CategoryId.From(request.Id), cancellationToken)
                       ?? throw new EntityNotFoundException(typeof(Category), request.Id);

        await _categoryRepository.RemoveAsync(category, cancellationToken);
    }
}
