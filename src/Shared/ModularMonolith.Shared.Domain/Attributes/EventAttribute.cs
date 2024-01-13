namespace ModularMonolith.Shared.Domain.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class EventAttribute : Attribute
{
    public EventAttribute(string name) => Name = name;

    public EventAttribute(string name, string? topic)
    {
        Name = name;
        Topic = topic;
    }

    public string Name { get; }

    public string? Topic { get; }
    
    public bool IsPersisted { get; init; }
}
