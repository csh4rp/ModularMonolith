using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModularMonolith.Shared.Domain.Entities;
using ModularMonolith.Shared.Infrastructure.Events.Options;

namespace ModularMonolith.Shared.Infrastructure.Events.DataAccess;

internal sealed class EventReader(IEventLogContext eventLogContext,
    TimeProvider timeProvider,
    IOptionsMonitor<EventOptions> options,
    ILogger<EventReader> logger)
{
    public async IAsyncEnumerable<EventLog> GetUnpublishedEventsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(options.CurrentValue.PollInterval);
        
        while (!cancellationToken.IsCancellationRequested)
        {
            await using var transaction = await eventLogContext.Database.BeginTransactionAsync(cancellationToken);
            
            var eventLogs = await GetLogsAsync(cancellationToken);

            if (eventLogs.Count == 0)
            {
                Extensions.EventLoggingExtensions.NoNewEvents(logger);
                
                await transaction.DisposeAsync();
                await timer.WaitForNextTickAsync(cancellationToken);
                continue;
            }
            
            Extensions.EventLoggingExtensions.FetchedNewEvents(logger, eventLogs.Count);
            
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
            
            Extensions.EventLoggingExtensions.EventsMarkedAsPublished(logger, ids);
        }
    }

    private async Task<List<EventLog>> GetLogsAsync(CancellationToken cancellationToken)
    {
        var limit = options.CurrentValue.BatchSize;
        var (tableName, idColumnName, publishedAtColumnName) = GetMetaData();
        
        var eventLogs = await eventLogContext.EventLogs.FromSql(
                $"""
                 SELECT *
                 FROM {tableName}
                 WHERE {publishedAtColumnName} IS NULL
                 ORDER BY {idColumnName} ASC
                 LIMIT {limit}
                 FOR UPDATE SKIP LOCKED
                 """)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        return eventLogs;
    }

    private (string TableName, string IdColumnetName, string PublishedAtColumnName) GetMetaData()
    {
        var eventLogEntityType = eventLogContext.Model.FindEntityType(typeof(EventLog))!;

        var tableName = eventLogEntityType.GetTableName();
        var schemeName = eventLogEntityType.GetSchema();

        var fullTableName = string.IsNullOrEmpty(schemeName)
            ? tableName
            : $"{schemeName}.{tableName}";

        var publishedAtProperty = eventLogEntityType.FindProperty(nameof(EventLog.PublishedAt))!;
        var idProperty = eventLogEntityType.FindProperty(nameof(EventLog.Id))!;

        return (fullTableName!, idProperty.GetColumnName(), publishedAtProperty.GetColumnName());
    }
}
