using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ModularMonolith.Shared.DataAccess.AudiLogs;
using ModularMonolith.Shared.DataAccess.EntityFramework.AuditLogs.Factories;

namespace ModularMonolith.Shared.DataAccess.EntityFramework.AuditLogs.Interceptors;

public class AuditLogInterceptor : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = new())
    {
        Debug.Assert(eventData.Context is not null);

        var logs = CreateLogs(eventData.Context);
        if (logs.Count == 0)
        {
            return result;
        }

        var store = eventData.Context.GetService<IAuditLogStore>();

        await store.AddRangeAsync(logs, cancellationToken);

        return result;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Debug.Assert(eventData.Context is not null);

        var logs = CreateLogs(eventData.Context);
        if (logs.Count == 0)
        {
            return result;
        }

        var store = eventData.Context.GetService<IAuditLogStore>();

        store.AddRangeAsync(logs, CancellationToken.None).GetAwaiter().GetResult();

        return result;
    }

    private static List<AuditLogEntry> CreateLogs(DbContext context)
    {
        var correlationId = context.Database.CurrentTransaction?.TransactionId ?? Guid.NewGuid();
        var factory = context.GetService<AuditLogFactory>();

        var changedEntities = context.ChangeTracker.Entries()
            .Where(e => e.State != EntityState.Detached && e.State != EntityState.Unchanged && e.IsAuditable())
            .ToList();
        
        return changedEntities.Select(e => factory.Create(e)).ToList();
    }
}
