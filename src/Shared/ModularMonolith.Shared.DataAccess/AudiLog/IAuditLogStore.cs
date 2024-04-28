using ModularMonolith.Shared.DataAccess.Models;

namespace ModularMonolith.Shared.DataAccess.AudiLog;

public interface IAuditLogStore
{
    Task AddAsync(AuditLogEntry entry, CancellationToken cancellationToken);

    Task AddRangeAsync(IEnumerable<AuditLogEntry> entries, CancellationToken cancellationToken);

    Task<DataPage<AuditLogEntry>> FindAsync(Paginator paginator,
        AuditLogSearchFilters filters,
        CancellationToken cancellationToken = new());

    IAsyncEnumerable<AuditLogEntry> FindAllAsync(AuditLogSearchFilters filters,
        CancellationToken cancellationToken);
}
