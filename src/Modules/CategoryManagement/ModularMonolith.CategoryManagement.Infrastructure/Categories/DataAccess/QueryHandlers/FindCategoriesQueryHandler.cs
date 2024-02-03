using ModularMonolith.CategoryManagement.Application.Categories.Abstract;
using ModularMonolith.CategoryManagement.Contracts.Categories.Models;
using ModularMonolith.CategoryManagement.Contracts.Categories.Queries;
using ModularMonolith.CategoryManagement.Contracts.Categories.Responses;
using ModularMonolith.Shared.Infrastructure.DataAccess.Extensions;
using ModularMonolith.Shared.Infrastructure.Queries;
using Z.EntityFramework.Plus;

namespace ModularMonolith.CategoryManagement.Infrastructure.Categories.DataAccess.QueryHandlers;

internal sealed class FindCategoriesQueryHandler : IQueryHandler<FindCategoriesQuery, CategoriesResponse>
{
    private readonly ICategoryDatabase _database;

    public FindCategoriesQueryHandler(ICategoryDatabase database) => _database = database;

    public async Task<CategoriesResponse> Handle(FindCategoriesQuery request, CancellationToken cancellationToken)
    {
        var query = _database.Categories.AsQueryable();

        if (!string.IsNullOrEmpty(request.Search))
        {
            query = query.Where(c => c.Name.StartsWith(request.Search));
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
