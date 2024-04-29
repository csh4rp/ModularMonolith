namespace ModularMonolith.Shared.Events;

[AttributeUsage(AttributeTargets.Class)]
public class EventAttribute : Attribute
{
    public EventAttribute(string name) => Name = name;

    public string Name { get; set; }

    public string? Topic { get; set; }

    public string? Target { get; set; }

    public bool IsPersisted { get; init; }
}
