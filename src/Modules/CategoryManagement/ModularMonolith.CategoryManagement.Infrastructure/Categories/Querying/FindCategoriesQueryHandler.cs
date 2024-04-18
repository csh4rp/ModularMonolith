using Microsoft.EntityFrameworkCore;
using ModularMonolith.CategoryManagement.Contracts.Categories.Querying;
using ModularMonolith.CategoryManagement.Contracts.Categories.Shared;
using ModularMonolith.CategoryManagement.Domain.Categories;
using ModularMonolith.Shared.DataAccess.Extensions;
using ModularMonolith.Shared.DataAccess.Queries;
using Z.EntityFramework.Plus;

namespace ModularMonolith.CategoryManagement.Infrastructure.Categories.Querying;

internal sealed class FindCategoriesQueryHandler : IQueryHandler<FindCategoriesQuery, CategoriesResponse>
{
    private readonly DbContext _database;

    public FindCategoriesQueryHandler(DbContext database) => _database = database;

    public async Task<CategoriesResponse> Handle(FindCategoriesQuery request, CancellationToken cancellationToken)
    {
        var query = _database.Set<Category>().AsQueryable();

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            query = query.Where(c => c.Name.StartsWith(request.SearchTerm));
        }

        var countFuture = query.DeferredCount().FutureValue();

        var itemsFuture = query
            .Select(c => new CategoryItemModel { Id = c.Id.Value, Name = c.Name })
            .ApplyPagination(request)
            .Future();

        var items = await itemsFuture.ToListAsync(cancellationToken);

        return new CategoriesResponse(items, countFuture);
    }
}
