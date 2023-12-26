using Microsoft.EntityFrameworkCore;
using ModularMonolith.CategoryManagement.Contracts.Categories.Queries;
using ModularMonolith.CategoryManagement.Contracts.Categories.Responses;
using ModularMonolith.CategoryManagement.Infrastructure.Common.DataAccess;
using ModularMonolith.Shared.Infrastructure.Queries;

namespace ModularMonolith.CategoryManagement.Infrastructure.Categories.DataAccess.QueryHandlers;

internal sealed class GetCategoryDetailsByIdQueryHandler
    : IQueryHandler<GetCategoryDetailsByIdQuery, CategoryDetailsResponse?>
{
    private readonly CategoryManagementDbContext _dbContext;

    public GetCategoryDetailsByIdQueryHandler(CategoryManagementDbContext dbContext) => _dbContext = dbContext;

    public Task<CategoryDetailsResponse?> Handle(GetCategoryDetailsByIdQuery request,
        CancellationToken cancellationToken) =>
        _dbContext.Categories.Where(c => c.Id == request.Id)
            .Select(c => new CategoryDetailsResponse { Id = c.Id, ParentId = c.ParentId, Name = c.Name })
            .FirstOrDefaultAsync(cancellationToken);
}
