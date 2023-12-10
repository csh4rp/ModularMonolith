namespace ModularMonolith.Shared.Infrastructure.Events.MetaData;

internal record EventInfo(Guid EventLogId, Guid? CorrelationId);
