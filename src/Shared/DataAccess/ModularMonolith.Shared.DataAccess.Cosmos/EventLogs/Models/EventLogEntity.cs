using Newtonsoft.Json.Linq;

namespace ModularMonolith.Shared.DataAccess.Cosmos.EventLogs.Models;

public class EventLogEntity
{
    public required Guid Id { get; init; }

    public required string PartitionKey { get; init; }

    public required DateTimeOffset Timestamp { get; init; }

    public required string EventTypeName { get; init; }

    public required JObject EventPayload { get; init; }

    public required EventLogEntityMetaData MetaData { get; init; }
}
