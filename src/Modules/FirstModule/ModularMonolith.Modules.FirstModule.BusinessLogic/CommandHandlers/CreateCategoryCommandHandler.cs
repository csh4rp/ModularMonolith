using ModularMonolith.Modules.FirstModule.BusinessLogic.Abstract;
using ModularMonolith.Modules.FirstModule.Contracts.Commands;
using ModularMonolith.Modules.FirstModule.Domain.Entities;
using ModularMonolith.Shared.BusinessLogic.Commands;

namespace ModularMonolith.Modules.FirstModule.BusinessLogic.CommandHandlers;

internal sealed class CreateCategoryCommandHandler : ICommandHandler<CreateCategoryCommand, Guid>
{
    private readonly ICategoryDatabase _categoryDatabase;

    public CreateCategoryCommandHandler(ICategoryDatabase categoryDatabase)
    {
        _categoryDatabase = categoryDatabase;
    }

    public async Task<Guid> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = new Category { ParentId = request.ParentId, Name = request.Name };

        _categoryDatabase.Categories.Add(category);

        await _categoryDatabase.SaveChangesAsync(cancellationToken);

        return category.Id;
    }
}
