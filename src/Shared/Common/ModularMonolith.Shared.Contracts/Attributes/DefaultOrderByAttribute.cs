namespace ModularMonolith.Shared.Contracts.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class DefaultOrderByAttribute : Attribute
{
    public DefaultOrderByAttribute(string propertyName, bool isAscending = true)
    {
        PropertyName = propertyName;
        IsAscending = isAscending;
    }

    public string PropertyName { get; }

    public bool IsAscending { get; }
}
