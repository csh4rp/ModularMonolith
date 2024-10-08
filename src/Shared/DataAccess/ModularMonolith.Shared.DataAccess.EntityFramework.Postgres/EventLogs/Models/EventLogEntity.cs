﻿using System.Text.Json;
using ModularMonolith.Shared.DataAccess.EntityFramework.Postgres.EventLog.Models;

namespace ModularMonolith.Shared.DataAccess.EntityFramework.Postgres.EventLogs.Models;

public class EventLogEntity
{
    public required Guid Id { get; init; }

    public required DateTimeOffset Timestamp { get; init; }

    public required string EventTypeName { get; init; }

    public required JsonDocument EventPayload { get; init; }

    public required EventLogEntityMetaData MetaData { get; init; }
}
