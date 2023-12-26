using ModularMonolith.CategoryManagement.Contracts.Categories.Models;
using ModularMonolith.CategoryManagement.Contracts.Categories.Queries;
using ModularMonolith.CategoryManagement.Contracts.Categories.Responses;
using ModularMonolith.CategoryManagement.Infrastructure.Common.DataAccess;
using ModularMonolith.Shared.Infrastructure.DataAccess.Extensions;
using ModularMonolith.Shared.Infrastructure.Queries;
using Z.EntityFramework.Plus;

namespace ModularMonolith.CategoryManagement.Infrastructure.Categories.DataAccess.QueryHandlers;

internal sealed class FindCategoriesQueryHandler : IQueryHandler<FindCategoriesQuery, CategoriesResponse>
{
    private readonly CategoryManagementDbContext _dbContext;

    public FindCategoriesQueryHandler(CategoryManagementDbContext dbContext) => _dbContext = dbContext;

    public async Task<CategoriesResponse> Handle(FindCategoriesQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Categories.AsQueryable();

        if (!string.IsNullOrEmpty(request.Search))
        {
            query = query.Where(c => c.Name.StartsWith(request.Search));
        }

        var countFuture = query.DeferredCount().FutureValue<int>();

        var itemsFuture = query
            .Select(c => new CategoryItemModel { Id = c.Id, Name = c.Name })
            .ApplyPagination(request)
            .Future();

        var items = await itemsFuture.ToListAsync(cancellationToken);

        return new CategoriesResponse { Items = items, TotalLength = countFuture.Value };
    }
}
