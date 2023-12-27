namespace ModularMonolith.Shared.IntegrationTests.Common;

public class TestMethodName : Attribute
{
    public string Name { get; }

    public TestMethodName(string name) => Name = name;
}
