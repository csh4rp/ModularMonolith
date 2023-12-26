namespace ModularMonolith.Shared.IntegrationTests.Common;

public class HasFileName : Attribute
{
    public string FileName { get; }

    public HasFileName(string fileName) => FileName = fileName;
}
