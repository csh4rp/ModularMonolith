namespace ModularMonolith.Tests.Utils.Abstractions;

public class TestFileNameAttribute : Attribute
{
    public string Name { get; }

    public TestFileNameAttribute(string name) => Name = name;
}
