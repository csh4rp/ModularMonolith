using Microsoft.EntityFrameworkCore;
using ModularMonolith.Modules.FirstModule.BusinessLogic.Abstract;
using ModularMonolith.Modules.FirstModule.Contracts.Commands;
using ModularMonolith.Modules.FirstModule.Domain.Entities;
using ModularMonolith.Shared.BusinessLogic.Commands;
using ModularMonolith.Shared.BusinessLogic.Exceptions;

namespace ModularMonolith.Modules.FirstModule.BusinessLogic.CommandHandlers;

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

        category.ParentId = request.ParentId;
        category.Name = request.Name;
        
        await _categoryDatabase.SaveChangesAsync(cancellationToken);
    }
}
