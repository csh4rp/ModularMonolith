using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;
using ModularMonolith.Shared.Contracts;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using Assembly = System.Reflection.Assembly;

namespace ModularMonolith.Shared.ArchitectureTests.Contracts;

public class ContractTests
{
    private static readonly Architecture Architecture = new ArchLoader().LoadAssemblies
    (
        Assembly.Load("ModularMonolith.CategoryManagement.Contracts"),
        Assembly.Load("ModularMonolith.Identity.Contracts")
    ).Build();

    [Fact]
    public void ShouldAllCommandsBePublicAndSealed()
    {
        var rule = Classes()
            .That()
            .AreAssignableTo(typeof(ICommand), typeof(ICommand<>))
            .Should()
            .BePublic()
            .AndShould()
            .BeSealed()
            .AndShould()
            .HaveNameEndingWith("Command");

        Architecture.CheckRule(rule);
    }

    [Fact]
    public void ShouldAllQueriesBePublicAndSealed()
    {
        var rule = Classes()
            .That()
            .AreAssignableTo(typeof(IQuery<>))
            .Should()
            .BePublic()
            .AndShould()
            .BeSealed()
            .AndShould()
            .HaveNameEndingWith("Query");

        Architecture.CheckRule(rule);
    }
}
