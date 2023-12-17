using ModularMonolith.Shared.Domain.Entities;
using ModularMonolith.Shared.Infrastructure.Events.MetaData;

namespace ModularMonolith.Shared.Infrastructure.Events.DataAccess.Abstract;

internal interface IEventReader
{
    Task<(bool WasAcquired, EventLog? EventLog)> TryAcquireLockAsync(EventInfo eventInfo, CancellationToken cancellationToken);
    
    Task MarkAsPublishedAsync(EventInfo eventInfo, CancellationToken cancellationToken);
    
    Task IncrementFailedAttemptNumberAsync(EventInfo eventInfo, CancellationToken cancellationToken);
    
    Task EnsureInitializedAsync(CancellationToken cancellationToken);
    
    Task<IReadOnlyList<EventInfo>> GetUnpublishedEventsAsync(CancellationToken cancellationToken);
}
