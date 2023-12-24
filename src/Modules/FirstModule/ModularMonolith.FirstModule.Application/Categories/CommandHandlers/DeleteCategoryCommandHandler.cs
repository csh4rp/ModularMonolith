using Microsoft.EntityFrameworkCore;
using ModularMonolith.FirstModule.Application.Categories.Abstract;
using ModularMonolith.FirstModule.Contracts.Categories.Commands;
using ModularMonolith.FirstModule.Domain.Entities;
using ModularMonolith.Shared.Application.Commands;
using ModularMonolith.Shared.Application.Exceptions;

namespace ModularMonolith.FirstModule.Application.Categories.CommandHandlers;

internal sealed class DeleteCategoryCommandHandler : ICommandHandler<DeleteCategoryCommand>
{
    private readonly ICategoryDatabase _categoryDatabase;

    public DeleteCategoryCommandHandler(ICategoryDatabase categoryDatabase) => _categoryDatabase = categoryDatabase;

    public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category =
            await _categoryDatabase.Categories.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new EntityNotFoundException(typeof(Category), request.Id);

        _categoryDatabase.Categories.Remove(category);

        await _categoryDatabase.SaveChangesAsync(cancellationToken);
    }
}
