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
        
        await foreach (var eventId in _eventChannel.ReadAllAsync(cancellationToken))
        {
            await using var scope = _serviceScopeFactory.CreateAsyncScope();
            await using var eventLogContext = scope.ServiceProvider.GetRequiredService<IEventLogContext>();
            
            await using var transaction = await eventLogContext.Database.BeginTransactionAsync(cancellationToken);
            
            var eventLog = await GetEventLogAsync(eventLogContext, eventId, cancellationToken);

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

    private Task<EventLog?> GetEventLogAsync(IEventLogContext eventLogContext, Guid eventId, CancellationToken cancellationToken)
    {
        var (tableName, idColumnName) = GetMetaData(eventLogContext);
        return eventLogContext.EventLogs.FromSql(
                $"""
                 SELECT *
                 FROM {tableName}
                 WHERE {idColumnName} = {eventId}
                 FOR UPDATE
                 SKIP LOCKED
                 """)
            .FirstOrDefaultAsync(cancellationToken);
    }
    
    private (string TableName, string IdColumnName) GetMetaData(IEventLogContext eventLogContext)
    {
        var eventLogEntityType = eventLogContext.Model.FindEntityType(typeof(EventLog))!;

        var tableName = eventLogEntityType.GetTableName();
        var schemeName = eventLogEntityType.GetSchema();

        var fullTableName = string.IsNullOrEmpty(schemeName)
            ? tableName
            : $"{schemeName}.{tableName}";
        
        var idProperty = eventLogEntityType.FindProperty(nameof(EventLog.Id))!;

        return (fullTableName!, idProperty.GetColumnName());
    }

    private async Task EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        await using var eventLogContext = scope.ServiceProvider.GetRequiredService<IEventLogContext>();
        
        var (tableName, idColumnName) = GetMetaData(eventLogContext);
        
        await eventLogContext.Database.ExecuteSqlAsync(
            $"""
            CREATE OR REPLACE FUNCTION notify_event_log_inserted() 
               RETURNS TRIGGER 
               LANGUAGE PLPGSQL
            AS
            $$
            BEGIN
               NOTIFY new_events, NEW.{idColumnName}::text
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
