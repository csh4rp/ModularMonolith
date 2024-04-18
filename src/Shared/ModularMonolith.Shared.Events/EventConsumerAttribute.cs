namespace ModularMonolith.Shared.Events;

[AttributeUsage(AttributeTargets.Class)]
public class EventConsumerAttribute : Attribute
{
    public string Queue { get; }

    public EventConsumerAttribute(string queue) => Queue = queue;
}
