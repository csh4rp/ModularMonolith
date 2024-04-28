using ModularMonolith.Shared.DataAccess.Models;

namespace ModularMonolith.Shared.DataAccess.EventLog;

public interface IEventLogStore
{
    Task AddAsync(EventLogEntry entry, CancellationToken cancellationToken);

    Task AddRangeAsync(IEnumerable<EventLogEntry> entries, CancellationToken cancellationToken);

    Task<DataPage<EventLogEntry>> FindAsync(Paginator paginator,
        EventLogSearchFilters filters,
        CancellationToken cancellationToken);

    IAsyncEnumerable<EventLogEntry> FindAllAsync(EventLogSearchFilters filters,
        CancellationToken cancellationToken);
}
