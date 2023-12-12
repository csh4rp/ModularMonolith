using System.Reflection;

namespace ModularMonolith.Shared.Infrastructure.Events.Options;

public class EventOptions
{
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(5);

    public List<Assembly> Assemblies { get; set; } = new();

    public int MaxParallelWorkers { get; set; } = Environment.ProcessorCount * 2;

    public int MaxRetryAttempts { get; set; } = 10;
    
    
}
