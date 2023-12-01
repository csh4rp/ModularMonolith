using Microsoft.EntityFrameworkCore;
using ModularMonolith.Modules.FirstModule.BusinessLogic.Abstract;
using ModularMonolith.Modules.FirstModule.Contracts.Commands;
using ModularMonolith.Modules.FirstModule.Domain.Entities;
using ModularMonolith.Shared.BusinessLogic.Commands;
using ModularMonolith.Shared.BusinessLogic.Exceptions;

namespace ModularMonolith.Modules.FirstModule.BusinessLogic.CommandHandlers;

internal sealed class DeleteCategoryCommandHandler : ICommandHandler<DeleteCategoryCommand>
{
    private readonly ICategoryDatabase _categoryDatabase;

    public DeleteCategoryCommandHandler(ICategoryDatabase categoryDatabase)
    {
        _categoryDatabase = categoryDatabase;
    }

    public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryDatabase.Categories.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
                       ?? throw new EntityNotFoundException(typeof(Category), request.Id);
        
        _categoryDatabase.Categories.Remove(category);

        await _categoryDatabase.SaveChangesAsync(cancellationToken);
    }
}
