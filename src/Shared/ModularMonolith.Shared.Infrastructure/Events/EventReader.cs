using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.Infrastructure.DataAccess;

namespace ModularMonolith.Shared.Infrastructure.Events;

public class EventReader
{
    private readonly BaseDbContext _eventLogContext;
    private readonly TimeProvider _timeProvider;

    public async IAsyncEnumerable<EventLog> GetUnpublishedEventsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var eventLogEntityType = _eventLogContext.Model.FindEntityType(typeof(EventLog))!;

        var tableName = eventLogEntityType.GetTableName();
        var schemeName = eventLogEntityType.GetSchema();

        var fullTableName = string.IsNullOrEmpty(schemeName)
            ? tableName
            : $"{schemeName}.{tableName}";

        var publishedAtProperty = eventLogEntityType.FindProperty(nameof(EventLog.PublishedAt))!;
        var idProperty = eventLogEntityType.FindProperty(nameof(EventLog.Id))!;

        while (!cancellationToken.IsCancellationRequested)
        {
            var transaction = await _eventLogContext.Database.BeginTransactionAsync(cancellationToken);
            
            var eventLogs = await _eventLogContext.EventLogs.FromSql(
                    $"""
                     SELECT *
                     FROM {fullTableName}
                     WHERE {publishedAtProperty.GetColumnName()} IS NULL
                     ORDER BY {idProperty.GetColumnName()}
                     LIMIT 10
                     FOR UPDATE SKIP LOCKED
                     """)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            foreach (var eventLog in eventLogs)
            {
                yield return eventLog;
            }

            var now = _timeProvider.GetUtcNow();
            var ids = eventLogs.Select(e => e.Id).ToList();

            _ = await _eventLogContext.EventLogs
                .Where(e => ids.Contains(e.Id))
                .ExecuteUpdateAsync(e => e.SetProperty(p => p.PublishedAt, now), cancellationToken);
            
            await transaction.CommitAsync(cancellationToken);
        }
        

    } 
}
