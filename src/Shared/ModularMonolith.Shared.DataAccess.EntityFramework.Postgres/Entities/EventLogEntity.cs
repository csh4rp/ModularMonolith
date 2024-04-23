using System.Text.Json;

namespace ModularMonolith.Shared.DataAccess.EntityFramework.Postgres.Entities;

public class EventLogEntity
{
    public required Guid Id { get; init; }

    public required DateTimeOffset Timestamp { get; init; }

    public required string EventTypeName { get; init; }

    public required JsonDocument EventPayload { get; init; }

    public required EventLogEntityMetaData MetaData { get; init; }
}
