using Microsoft.EntityFrameworkCore;
using ModularMonolith.CategoryManagement.Application.Categories.Abstract;
using ModularMonolith.CategoryManagement.Contracts.Categories.Details;
using ModularMonolith.CategoryManagement.Domain.Categories;
using ModularMonolith.Shared.Application.Exceptions;
using ModularMonolith.Shared.Infrastructure.Queries;

namespace ModularMonolith.CategoryManagement.Infrastructure.Categories.DataAccess.QueryHandlers;

internal sealed class GetCategoryDetailsByIdQueryHandler
    : IQueryHandler<GetCategoryDetailsByIdQuery, CategoryDetailsResponse>
{
    private readonly ICategoryDatabase _database;

    public GetCategoryDetailsByIdQueryHandler(ICategoryDatabase database) => _database = database;

    public async Task<CategoryDetailsResponse> Handle(GetCategoryDetailsByIdQuery request,
        CancellationToken cancellationToken) =>
        await _database.Categories.Where(c => c.Id.Value == request.Id)
            .Select(c => new CategoryDetailsResponse
            {
                Id = c.Id.Value,
                ParentId = c.ParentId.HasValue ? c.ParentId.Value.Value : null,
                Name = c.Name
            })
            .FirstOrDefaultAsync(cancellationToken)
        ?? throw new EntityNotFoundException(typeof(Category), request.Id);
}
