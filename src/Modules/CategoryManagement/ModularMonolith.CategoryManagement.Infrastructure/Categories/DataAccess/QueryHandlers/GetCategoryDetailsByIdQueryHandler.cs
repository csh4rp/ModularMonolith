using Microsoft.EntityFrameworkCore;
using ModularMonolith.CategoryManagement.Contracts.Categories.Details;
using ModularMonolith.CategoryManagement.Domain.Categories;
using ModularMonolith.Shared.Application.Exceptions;
using ModularMonolith.Shared.Infrastructure.Queries;

namespace ModularMonolith.CategoryManagement.Infrastructure.Categories.DataAccess.QueryHandlers;

internal sealed class GetCategoryDetailsByIdQueryHandler
    : IQueryHandler<GetCategoryDetailsByIdQuery, CategoryDetailsResponse>
{
    private readonly DbContext _database;

    public GetCategoryDetailsByIdQueryHandler(DbContext database) => _database = database;

    public async Task<CategoryDetailsResponse> Handle(GetCategoryDetailsByIdQuery request,
        CancellationToken cancellationToken) =>
        await _database.Set<Category>().Where(c => c.Id.Value == request.Id)
            .Select(c => new CategoryDetailsResponse
            {
                Id = c.Id.Value,
                ParentId = c.ParentId.HasValue ? c.ParentId.Value.Value : null,
                Name = c.Name
            })
            .FirstOrDefaultAsync(cancellationToken)
        ?? throw new EntityNotFoundException(typeof(Category), request.Id);
}
