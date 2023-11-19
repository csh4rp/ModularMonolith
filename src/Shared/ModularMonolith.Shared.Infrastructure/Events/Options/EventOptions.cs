using System.Reflection;

namespace ModularMonolith.Shared.Infrastructure.Events.Options;

public class EventOptions
{
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(5);

    public int BatchSize { get; set; } = 10;

    public List<Assembly> Assemblies { get; set; } = new();
    
}
