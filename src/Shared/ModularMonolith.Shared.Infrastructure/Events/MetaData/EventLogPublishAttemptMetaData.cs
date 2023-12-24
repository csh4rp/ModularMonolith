namespace ModularMonolith.Shared.Infrastructure.Events.MetaData;

public class EventLogPublishAttemptMetaData
{
    public required string TableName { get; init; }

    public required string EventLogIdColumnName { get; init; }

    public required string AttemptNumberColumnName { get; init; }

    public required string NextAttemptAtColumnName { get; init; }
}
