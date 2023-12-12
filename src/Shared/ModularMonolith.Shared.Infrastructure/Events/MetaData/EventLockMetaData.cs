﻿namespace ModularMonolith.Shared.Infrastructure.Events.MetaData;

public class EventLockMetaData
{
    public required string TableName { get; init; }

    public required string IdColumnName { get; init; }
    
    public required string AcquiredAtColumnName { get; init; }
}
