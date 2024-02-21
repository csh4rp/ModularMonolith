using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;
using ModularMonolith.Shared.Api;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using Assembly = System.Reflection.Assembly;

namespace ModularMonolith.Shared.ArchitectureTests.Api;

public class ApiTests
{
    private static readonly Architecture Architecture = new ArchLoader().LoadAssemblies
    (
        Assembly.Load("ModularMonolith.Shared.Api"),
        Assembly.Load("ModularMonolith.CategoryManagement.Api"),
        Assembly.Load("ModularMonolith.Identity.Api")
    ).Build();

    [Fact]
    public void ShouldAllModuleClassesBePublic()
    {
        var rule = Classes()
            .That()
            .AreAssignableTo(typeof(AppModule))
            .Should()
            .BePublic();

        Architecture.CheckRule(rule);
    }
}
