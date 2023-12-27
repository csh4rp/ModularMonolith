namespace ModularMonolith.Shared.Contracts.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public sealed class DefaultTakeAttribute : Attribute
{
    public DefaultTakeAttribute(int take) => Take = take;

    public int Take { get; }
}
