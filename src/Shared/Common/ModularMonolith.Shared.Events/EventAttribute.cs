namespace ModularMonolith.Shared.Events;

[AttributeUsage(AttributeTargets.Class)]
public class EventAttribute : Attribute
{
    public EventAttribute(string name) => Name = name;

    public string Name { get; }

    public bool IsPersisted { get; set; }
}
