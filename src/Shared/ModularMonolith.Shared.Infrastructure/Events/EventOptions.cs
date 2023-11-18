namespace ModularMonolith.Shared.Infrastructure.Events;

public class EventOptions
{
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(5);

    public int BatchSize { get; set; } = 10;
}
