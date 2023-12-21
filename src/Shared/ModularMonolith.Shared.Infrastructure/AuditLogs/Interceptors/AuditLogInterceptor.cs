using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ModularMonolith.Shared.Domain.Entities;
using ModularMonolith.Shared.Infrastructure.AuditLogs.Extensions;
using ModularMonolith.Shared.Infrastructure.AuditLogs.Factories;

namespace ModularMonolith.Shared.Infrastructure.AuditLogs.Interceptors;

public sealed class AuditLogInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = new())
    {
        Debug.Assert(eventData.Context is not null);

        AddAuditLogs(eventData.Context);

        return new ValueTask<InterceptionResult<int>>(result);
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Debug.Assert(eventData.Context is not null);

        AddAuditLogs(eventData.Context);

        return result;
    }

    private static void AddAuditLogs(DbContext context)
    {
        var changedEntities = context.ChangeTracker.Entries()
            .Where(e => e.State != EntityState.Detached && e.State != EntityState.Unchanged && e.IsAuditable());

        var factory = context.GetService<AuditLogFactory>();

        var logs = changedEntities.Select(factory.Create).ToList();

        context.Set<AuditLog>().AddRange(logs);
    }
}
