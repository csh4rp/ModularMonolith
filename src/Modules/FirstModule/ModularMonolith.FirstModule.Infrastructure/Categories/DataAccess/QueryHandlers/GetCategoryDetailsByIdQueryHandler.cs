using Microsoft.EntityFrameworkCore;
using ModularMonolith.FirstModule.Contracts.Categories.Queries;
using ModularMonolith.FirstModule.Contracts.Categories.Responses;
using ModularMonolith.FirstModule.Infrastructure.Common;
using ModularMonolith.FirstModule.Infrastructure.Common.DataAccess;
using ModularMonolith.Shared.Infrastructure.Queries;

namespace ModularMonolith.FirstModule.Infrastructure.Categories.DataAccess.QueryHandlers;

internal sealed class GetCategoryDetailsByIdQueryHandler
    : IQueryHandler<GetCategoryDetailsByIdQuery, CategoryDetailsResponse?>
{
    private readonly FirstModuleDbContext _dbContext;

    public GetCategoryDetailsByIdQueryHandler(FirstModuleDbContext dbContext) => _dbContext = dbContext;

    public Task<CategoryDetailsResponse?> Handle(GetCategoryDetailsByIdQuery request,
        CancellationToken cancellationToken) =>
        _dbContext.Categories.Where(c => c.Id == request.Id)
            .Select(c => new CategoryDetailsResponse { Id = c.Id, ParentId = c.ParentId, Name = c.Name })
            .FirstOrDefaultAsync(cancellationToken);
}
