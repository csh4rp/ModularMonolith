using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;
using ModularMonolith.Shared.Infrastructure.Queries;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using Assembly = System.Reflection.Assembly;

namespace ModularMonolith.Shared.ArchitectureTests.Infrastructure;

public class InfrastructureTests
{
    private static readonly Architecture Architecture = new ArchLoader().LoadAssemblies
    (
        Assembly.Load("ModularMonolith.CategoryManagement.Infrastructure"),
        Assembly.Load("ModularMonolith.Identity.Infrastructure")
    ).Build();

    [Fact]
    public void ShouldAllQueryHandlersBeInternalAndSealed()
    {
        var rule = Classes()
            .That()
            .AreAssignableTo(typeof(IQueryHandler<,>))
            .Should()
            .BeInternal()
            .AndShould()
            .BeSealed()
            .AndShould()
            .HaveNameEndingWith("Handler");

        Architecture.CheckRule(rule);
    }
}
