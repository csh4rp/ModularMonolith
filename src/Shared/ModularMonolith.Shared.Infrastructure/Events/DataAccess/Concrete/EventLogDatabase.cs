using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.Domain.Entities;
using ModularMonolith.Shared.Infrastructure.Events.DataAccess.Abstract;

namespace ModularMonolith.Shared.Infrastructure.Events.DataAccess.Concrete;

internal sealed class EventLogDatabase : IEventLogDatabase
{
    private readonly DbContext _dbContext;

    public EventLogDatabase(DbContext dbContext) => _dbContext = dbContext;

    public DbSet<EventLog> EventLogs => _dbContext.Set<EventLog>();

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}
