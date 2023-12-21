using System.Reflection;

namespace ModularMonolith.Shared.Infrastructure.Events.Options;

public class EventOptions
{
    public TimeSpan PollInterval { get; set; }

    public TimeSpan MaxLockTime { get; set; }

    public List<Assembly> Assemblies { get; set; } = new();

    public int MaxParallelWorkers { get; set; }

    public int MaxRetryAttempts { get; set; }
}
