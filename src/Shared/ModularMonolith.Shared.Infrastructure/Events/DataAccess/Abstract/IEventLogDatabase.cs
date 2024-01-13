using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.Domain.Entities;

namespace ModularMonolith.Shared.Infrastructure.Events.DataAccess.Abstract;

public interface IEventLogDatabase
{
    DbSet<EventLog> EventLogs { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
