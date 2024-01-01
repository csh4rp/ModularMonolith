namespace ModularMonolith.Shared.IntegrationTests.Common;

public class TestFileNameAttribute : Attribute
{
    public string Name { get; }

    public TestFileNameAttribute(string name) => Name = name;
}
