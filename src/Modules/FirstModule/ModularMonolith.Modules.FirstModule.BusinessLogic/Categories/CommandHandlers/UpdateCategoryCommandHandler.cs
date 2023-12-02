using Microsoft.EntityFrameworkCore;
using ModularMonolith.Modules.FirstModule.BusinessLogic.Categories.Abstract;
using ModularMonolith.Modules.FirstModule.Contracts.Categories.Commands;
using ModularMonolith.Modules.FirstModule.Domain.Entities;
using ModularMonolith.Shared.BusinessLogic.Commands;
using ModularMonolith.Shared.BusinessLogic.Exceptions;
using ModularMonolith.Shared.Contracts.Errors;

namespace ModularMonolith.Modules.FirstModule.BusinessLogic.Categories.CommandHandlers;

internal sealed class UpdateCategoryCommandHandler : ICommandHandler<UpdateCategoryCommand>
{
    private readonly ICategoryDatabase _categoryDatabase;

    public UpdateCategoryCommandHandler(ICategoryDatabase categoryDatabase)
    {
        _categoryDatabase = categoryDatabase;
    }

    public async Task Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryDatabase.Categories.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new EntityNotFoundException(typeof(Category), request.Id);
        
        var categoryWithNameExists = await _categoryDatabase.Categories
            .AnyAsync(c => c.Id != request.Id && c.Name == request.Name, cancellationToken);

        if (categoryWithNameExists)
        {
            throw new ValidationException(PropertyError.NotUnique(nameof(request.Name), request.Name));
        }

        category.ParentId = request.ParentId;
        category.Name = request.Name;
        
        await _categoryDatabase.SaveChangesAsync(cancellationToken);
    }
}
