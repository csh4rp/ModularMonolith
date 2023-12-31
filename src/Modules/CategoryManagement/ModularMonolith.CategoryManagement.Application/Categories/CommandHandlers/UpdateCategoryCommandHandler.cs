using Microsoft.EntityFrameworkCore;
using ModularMonolith.CategoryManagement.Application.Categories.Abstract;
using ModularMonolith.CategoryManagement.Contracts.Categories.Commands;
using ModularMonolith.CategoryManagement.Domain.Entities;
using ModularMonolith.Shared.Application.Commands;
using ModularMonolith.Shared.Contracts;
using ModularMonolith.Shared.Contracts.Errors;

namespace ModularMonolith.CategoryManagement.Application.Categories.CommandHandlers;

internal sealed class UpdateCategoryCommandHandler : ICommandHandler<UpdateCategoryCommand>
{
    private readonly ICategoryDatabase _categoryDatabase;

    public UpdateCategoryCommandHandler(ICategoryDatabase categoryDatabase) => _categoryDatabase = categoryDatabase;

    public async Task<Result> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryDatabase.Categories
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category is null)
        {
            return new EntityNotFoundError(nameof(Category), request.Id);
        }
        
        var categoryWithNameExists = await _categoryDatabase.Categories
            .AnyAsync(c => c.Id != request.Id && c.Name == request.Name, cancellationToken);

        if (categoryWithNameExists)
        {
            return new ConflictError(nameof(request.Name));
        }
        
        if (request.ParentId.HasValue)
        {
            var parentExists = await _categoryDatabase.Categories
                .AnyAsync(c => c.Id == request.ParentId, cancellationToken);

            if (!parentExists)
            {
                return MemberError.InvalidValue(nameof(UpdateCategoryCommand.ParentId));
            }
        }

        category.ParentId = request.ParentId;
        category.Name = request.Name;

        await _categoryDatabase.SaveChangesAsync(cancellationToken);

        return Result.Successful;
    }
}
