using ModularMonolith.Shared.DataAccess.Models;

namespace ModularMonolith.Shared.DataAccess.AudiLog;

public interface IAuditLogStore
{
    Task AddAsync(AuditLogEntry entry, CancellationToken cancellationToken = new());

    Task AddRangeAsync(IEnumerable<AuditLogEntry> entries, CancellationToken cancellationToken = new());

    Task<DataPage<AuditLogEntry>> FindAsync(Paginator<AuditLogEntry> paginator,
        AuditLogSearchFilters? filters = null,
        CancellationToken cancellationToken = new());

    IAsyncEnumerable<AuditLogEntry> FindAllAsync(AuditLogSearchFilters? filters = null,
        CancellationToken cancellationToken = new());
}
