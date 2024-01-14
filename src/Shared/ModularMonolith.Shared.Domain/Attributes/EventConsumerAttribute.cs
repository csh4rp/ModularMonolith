namespace ModularMonolith.Shared.Domain.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class EventConsumerAttribute : Attribute
{
    public string Queue { get; }

    public EventConsumerAttribute(string queue) => Queue = queue;
}
