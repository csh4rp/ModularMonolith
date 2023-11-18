using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.Infrastructure.AuditLogs;

namespace ModularMonolith.Shared.Infrastructure.DataAccess;

public interface IAuditLogContext
{
    DbSet<AuditLog> AuditLogs { get; }
}
