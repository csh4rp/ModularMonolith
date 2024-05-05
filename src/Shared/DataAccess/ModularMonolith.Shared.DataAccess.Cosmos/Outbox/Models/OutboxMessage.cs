using Newtonsoft.Json.Linq;

namespace ModularMonolith.Shared.DataAccess.Cosmos.Outbox.Models;

public class OutboxMessage
{
    public required Guid Id { get; init; }

    public required DateTimeOffset Timestamp { get; init; }

    public required string MessageTypeName { get; init; }

    public required JObject MessagePayload { get; init; }
}
