using ModularMonolith.Shared.DataAccess.Models;

namespace ModularMonolith.Shared.DataAccess.EventLog;

public interface IEventLogStore
{
    Task AddAsync(EventLogEntry entry, CancellationToken cancellationToken = new());

    Task AddRangeAsync(IEnumerable<EventLogEntry> entries, CancellationToken cancellationToken = new());

    Task<DataPage<EventLogEntry>> FindAsync(Paginator<EventLogEntry> paginator,
        EventLogSearchFilters? filters = null,
        CancellationToken cancellationToken = new());

    IAsyncEnumerable<EventLogEntry> FindAllAsync(EventLogSearchFilters? filters = null,
        CancellationToken cancellationToken = new());
}
