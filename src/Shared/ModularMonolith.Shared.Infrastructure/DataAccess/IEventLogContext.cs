using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using ModularMonolith.Shared.Infrastructure.Events;

namespace ModularMonolith.Shared.Infrastructure.DataAccess;

public interface IEventLogContext
{
    DbSet<EventLog> EventLogs { get; }
    
    IModel Model { get; }
    
    DatabaseFacade Database { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
