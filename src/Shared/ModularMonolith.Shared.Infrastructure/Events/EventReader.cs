using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ModularMonolith.Shared.Infrastructure.DataAccess;

namespace ModularMonolith.Shared.Infrastructure.Events;

public sealed class EventReader(IEventLogContext eventLogContext, TimeProvider timeProvider, IOptionsMonitor<EventOptions> options)
{
    public async IAsyncEnumerable<EventLog> GetUnpublishedEventsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var eventLogEntityType = eventLogContext.Model.FindEntityType(typeof(EventLog))!;

        var tableName = eventLogEntityType.GetTableName();
        var schemeName = eventLogEntityType.GetSchema();

        var fullTableName = string.IsNullOrEmpty(schemeName)
            ? tableName
            : $"{schemeName}.{tableName}";

        var publishedAtProperty = eventLogEntityType.FindProperty(nameof(EventLog.PublishedAt))!;
        var idProperty = eventLogEntityType.FindProperty(nameof(EventLog.Id))!;

        while (!cancellationToken.IsCancellationRequested)
        {
            var limit = options.CurrentValue.BatchSize;
            
            await using var transaction = await eventLogContext.Database.BeginTransactionAsync(cancellationToken);
            
            var eventLogs = await eventLogContext.EventLogs.FromSql(
                    $"""
                     SELECT *
                     FROM {fullTableName}
                     WHERE {publishedAtProperty.GetColumnName()} IS NULL
                     ORDER BY {idProperty.GetColumnName()}
                     LIMIT {limit}
                     FOR UPDATE SKIP LOCKED
                     """)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            if (eventLogs.Count == 0)
            {
                await transaction.DisposeAsync();
                await Task.Delay(options.CurrentValue.PollInterval, cancellationToken);
                continue;
            }
            
            foreach (var eventLog in eventLogs)
            {
                yield return eventLog;
            }

            var now = timeProvider.GetUtcNow();
            var ids = eventLogs.Select(e => e.Id).ToList();

            _ = await eventLogContext.EventLogs
                .Where(e => ids.Contains(e.Id))
                .ExecuteUpdateAsync(e => e.SetProperty(p => p.PublishedAt, now), cancellationToken);
            
            await transaction.CommitAsync(cancellationToken);
        }
    } 
}
