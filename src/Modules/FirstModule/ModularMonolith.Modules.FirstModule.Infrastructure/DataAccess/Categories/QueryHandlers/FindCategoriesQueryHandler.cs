using ModularMonolith.Modules.FirstModule.Contracts.Categories.Models;
using ModularMonolith.Modules.FirstModule.Contracts.Categories.Queries;
using ModularMonolith.Modules.FirstModule.Contracts.Categories.Responses;
using ModularMonolith.Shared.Infrastructure.DataAccess.Extensions;
using ModularMonolith.Shared.Infrastructure.Queries;
using Z.EntityFramework.Plus;

namespace ModularMonolith.Modules.FirstModule.Infrastructure.DataAccess.Categories.QueryHandlers;

internal sealed class FindCategoriesQueryHandler : IQueryHandler<FindCategoriesQuery, CategoriesResponse>
{
    private readonly FirstModuleDbContext _dbContext;

    public FindCategoriesQueryHandler(FirstModuleDbContext dbContext) => _dbContext = dbContext;

    public async Task<CategoriesResponse> Handle(FindCategoriesQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Categories.AsQueryable();

        if (!string.IsNullOrEmpty(request.Search))
        {
            query = query.Where(c => c.Name.StartsWith(request.Search));
        }

        var countFuture = query.DeferredCount().FutureValue();

        var itemsFuture = query
            .Select(c => new CategoryItemModel { Id = c.Id, Name = c.Name })
            .ApplyPagination(request)
            .Future();

        var items = await itemsFuture.ToListAsync(cancellationToken);

        return new CategoriesResponse { Items = items, TotalLength = countFuture };
    }
}
