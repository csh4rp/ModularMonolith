namespace ModularMonolith.Shared.Events;

[AttributeUsage(AttributeTargets.Class)]
public class EventConsumerAttribute : Attribute
{
    public EventConsumerAttribute(string consumerName) => ConsumerName = consumerName;

    public string ConsumerName { get; }
}
