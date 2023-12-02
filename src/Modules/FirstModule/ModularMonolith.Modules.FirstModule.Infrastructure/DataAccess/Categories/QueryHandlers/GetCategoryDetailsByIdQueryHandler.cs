using Microsoft.EntityFrameworkCore;
using ModularMonolith.Modules.FirstModule.Contracts.Categories.Queries;
using ModularMonolith.Modules.FirstModule.Contracts.Categories.Responses;
using ModularMonolith.Shared.Infrastructure.Queries;

namespace ModularMonolith.Modules.FirstModule.Infrastructure.DataAccess.Categories.QueryHandlers;

internal sealed class GetCategoryDetailsByIdQueryHandler : IQueryHandler<GetCategoryDetailsByIdQuery, CategoryDetailsResponse?>
{
    private readonly CategoryDbContext _dbContext;

    public GetCategoryDetailsByIdQueryHandler(CategoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<CategoryDetailsResponse?> Handle(GetCategoryDetailsByIdQuery request, CancellationToken cancellationToken) =>
        _dbContext.Categories.Where(c => c.Id == request.Id)
            .Select(c => new CategoryDetailsResponse { Id = c.Id, ParentId = c.ParentId, Name = c.Name, })
            .FirstOrDefaultAsync(cancellationToken);
}
