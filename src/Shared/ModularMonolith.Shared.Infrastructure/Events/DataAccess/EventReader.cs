using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModularMonolith.Shared.Domain.Entities;
using ModularMonolith.Shared.Infrastructure.Events.Extensions;
using ModularMonolith.Shared.Infrastructure.Events.Utils;

namespace ModularMonolith.Shared.Infrastructure.Events.DataAccess;

internal sealed class EventReader
{
    private readonly TimeProvider _timeProvider;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly EventChannel _eventChannel;
    private readonly ILogger<EventReader> _logger;

    public EventReader(TimeProvider timeProvider,
        IServiceScopeFactory serviceScopeFactory,
        EventChannel eventChannel,
        ILogger<EventReader> logger)
    {
        _timeProvider = timeProvider;
        _serviceScopeFactory = serviceScopeFactory;
        _eventChannel = eventChannel;
        _logger = logger;
    }

    public async IAsyncEnumerable<EventLog> GetUnpublishedEventsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await EnsureInitializedAsync(cancellationToken);
        
        await foreach (var eventInfo in _eventChannel.ReadAllAsync(cancellationToken))
        {
            await using var scope = _serviceScopeFactory.CreateAsyncScope();
            await using var eventLogContext = scope.ServiceProvider.GetRequiredService<IEventLogContext>();
            
            await using var transaction = await eventLogContext.Database.BeginTransactionAsync(cancellationToken);
            
            var eventLog = await GetEventLogAsync(eventLogContext, eventInfo, cancellationToken);

            if (eventLog is null)
            {
                _logger.EventAlreadyTaken(eventId);
                
                await transaction.DisposeAsync();
                continue;
            }

            yield return eventLog;
            
            eventLog.MarkAsPublished(_timeProvider.GetUtcNow());

            await eventLogContext.SaveChangesAsync(cancellationToken);
            
            await transaction.CommitAsync(cancellationToken);
        }
    }

    private Task<EventLog?> GetEventLogAsync(IEventLogContext eventLogContext, EventInfo eventInfo, CancellationToken cancellationToken)
    {
        var (tableName, idColumnName, correlationIdColumnName) = GetMetaData(eventLogContext);

        if (eventInfo.CorrelationId.HasValue)
        {
            return eventLogContext.EventLogs.FromSql(
                $"""
                 SELECT *
                 FROM {tableName}
                 WHERE {idColumnName} = {eventInfo.EventLogId}
                 AND {correlationIdColumnName} IS NULL
                 FOR UPDATE
                 SKIP LOCKED
                 """)
                .FirstOrDefaultAsync(cancellationToken);
        }
        
        return eventLogContext.EventLogs.FromSql(
                $"""
                 SELECT *
                 FROM {tableName}
                 WHERE {idColumnName} = {eventInfo.EventLogId}
                 AND {correlationIdColumnName} IS NULL
                 FOR UPDATE
                 SKIP LOCKED
                 """)
            .FirstOrDefaultAsync(cancellationToken);
    }
    
    private (string TableName, string IdColumnName, string CorrelationIdColumnName) GetMetaData(IEventLogContext eventLogContext)
    {
        var eventLogEntityType = eventLogContext.Model.FindEntityType(typeof(EventLog))!;

        var tableName = eventLogEntityType.GetTableName();
        var schemeName = eventLogEntityType.GetSchema();

        var fullTableName = string.IsNullOrEmpty(schemeName)
            ? tableName
            : $"{schemeName}.{tableName}";
        
        var idProperty = eventLogEntityType.FindProperty(nameof(EventLog.Id))!;

        var correlationIdProperty = eventLogEntityType.FindProperty(nameof(EventLog.CorrelationId));
        
        return (fullTableName!, idProperty.GetColumnName(), correlationIdProperty!.GetColumnName());
    }

    private async Task EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        await using var eventLogContext = scope.ServiceProvider.GetRequiredService<IEventLogContext>();
        
        var (tableName, idColumnName, correlationIdColumnName) = GetMetaData(eventLogContext);
        
        await eventLogContext.Database.ExecuteSqlAsync(
            $"""
            CREATE OR REPLACE FUNCTION notify_event_log_inserted() 
               RETURNS TRIGGER 
               LANGUAGE PLPGSQL
            AS
            $$
            BEGIN
               NOTIFY new_events, CONCAT(COALESCE(NEW.{correlationIdColumnName}::text, ''), '/', NEW.{idColumnName}::text)
               RETURN NEW;
            END;
            $$

            CREATE OR REPLACE TRIGGER event_inserted
                AFTER INSERT ON {tableName}
                FOR EACH ROW
                EXECUTE FUNCTION notify_event_log_inserted()
            """, cancellationToken);
    }
}
