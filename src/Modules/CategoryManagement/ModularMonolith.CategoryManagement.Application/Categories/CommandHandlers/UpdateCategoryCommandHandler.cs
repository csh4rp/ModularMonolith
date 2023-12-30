using Microsoft.EntityFrameworkCore;
using ModularMonolith.CategoryManagement.Application.Categories.Abstract;
using ModularMonolith.CategoryManagement.Contracts.Categories.Commands;
using ModularMonolith.CategoryManagement.Domain.Entities;
using ModularMonolith.Shared.Application.Commands;
using ModularMonolith.Shared.Application.Exceptions;
using ModularMonolith.Shared.Contracts;
using ModularMonolith.Shared.Contracts.Errors;

namespace ModularMonolith.CategoryManagement.Application.Categories.CommandHandlers;

internal sealed class UpdateCategoryCommandHandler : ICommandHandler<UpdateCategoryCommand>
{
    private readonly ICategoryDatabase _categoryDatabase;

    public UpdateCategoryCommandHandler(ICategoryDatabase categoryDatabase) => _categoryDatabase = categoryDatabase;

    public async Task Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category =
            await _categoryDatabase.Categories.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new EntityNotFoundException(typeof(Category), request.Id);

        var categoryWithNameExists = await _categoryDatabase.Categories
            .AnyAsync(c => c.Id != request.Id && c.Name == request.Name, cancellationToken);

        if (categoryWithNameExists)
        {
            throw new ConflictException(nameof(request.Name), ErrorCodes.NotUnique, "A category with given name already exists.");
        }
        
        if (request.ParentId.HasValue)
        {
            var parentExists = await _categoryDatabase.Categories
                .AnyAsync(c => c.Id == request.ParentId, cancellationToken);

            if (!parentExists)
            {
                throw new ValidationException(
                    PropertyError.InvalidArgument(nameof(CreateCategoryCommand.ParentId), request.ParentId));
            }
        }

        category.ParentId = request.ParentId;
        category.Name = request.Name;

        await _categoryDatabase.SaveChangesAsync(cancellationToken);
    }
}
