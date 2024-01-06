using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using ModularMonolith.Shared.Domain.Entities;

namespace ModularMonolith.Shared.Infrastructure.Events.DataAccess.Abstract;

public interface IEventLogDbContext : IDisposable, IAsyncDisposable
{
    DbSet<EventLog> EventLogs { get; }

    DbSet<EventLogLock> EventLogLocks { get; }
    DbSet<EventCorrelationLock> EventCorrelationLocks { get; }

    DbSet<EventLogPublishAttempt> EventLogPublishAttempts { get; }

    IModel Model { get; }

    DatabaseFacade Database { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
