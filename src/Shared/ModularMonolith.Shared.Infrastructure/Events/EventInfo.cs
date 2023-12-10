namespace ModularMonolith.Shared.Infrastructure.Events;

public record EventInfo(Guid EventLogId, Guid? CorrelationId);
