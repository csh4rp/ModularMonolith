using ModularMonolith.Shared.Domain.Entities;
using ModularMonolith.Shared.Infrastructure.Events.MetaData;

namespace ModularMonolith.Shared.Infrastructure.Events.DataAccess.Abstract;

internal interface IEventStore
{
    Task<(bool WasLockAcquired, EventLog? EventLog)> TryAcquireLockAsync(EventInfo eventInfo,
        CancellationToken cancellationToken);

    Task MarkAsPublishedAsync(EventInfo eventInfo, CancellationToken cancellationToken);

    Task AddFailedAttemptAsync(EventInfo eventInfo, CancellationToken cancellationToken);

    Task<IReadOnlyList<EventInfo>> GetUnpublishedEventsAsync(int take, CancellationToken cancellationToken);
}
