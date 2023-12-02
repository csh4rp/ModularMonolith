using Microsoft.EntityFrameworkCore;
using ModularMonolith.Modules.FirstModule.BusinessLogic.Categories.Abstract;
using ModularMonolith.Modules.FirstModule.Contracts.Categories.Commands;
using ModularMonolith.Modules.FirstModule.Domain.Entities;
using ModularMonolith.Shared.BusinessLogic.Commands;
using ModularMonolith.Shared.BusinessLogic.Exceptions;
using ModularMonolith.Shared.Contracts.Errors;

namespace ModularMonolith.Modules.FirstModule.BusinessLogic.Categories.CommandHandlers;

internal sealed class CreateCategoryCommandHandler : ICommandHandler<CreateCategoryCommand, Guid>
{
    private readonly ICategoryDatabase _categoryDatabase;

    public CreateCategoryCommandHandler(ICategoryDatabase categoryDatabase)
    {
        _categoryDatabase = categoryDatabase;
    }

    public async Task<Guid> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var categoryWithNameExists = await _categoryDatabase.Categories
            .AnyAsync(c => c.Name == request.Name, cancellationToken);

        if (categoryWithNameExists)
        {
            throw new ValidationException(PropertyError.NotUnique(nameof(request.Name), request.Name));
        }
        
        var category = new Category { ParentId = request.ParentId, Name = request.Name };

        _categoryDatabase.Categories.Add(category);

        await _categoryDatabase.SaveChangesAsync(cancellationToken);

        return category.Id;
    }
}
