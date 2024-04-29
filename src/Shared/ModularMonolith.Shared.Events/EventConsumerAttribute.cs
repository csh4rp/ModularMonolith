namespace ModularMonolith.Shared.Events;

[AttributeUsage(AttributeTargets.Class)]
public class EventConsumerAttribute : Attribute
{
    public string ConsumerName { get; }

    public string? Target { get; set; }

    public EventConsumerAttribute(string consumerName) => ConsumerName = consumerName;
}
