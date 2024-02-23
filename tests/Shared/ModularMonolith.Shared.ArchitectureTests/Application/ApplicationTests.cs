using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;
using FluentValidation;
using ModularMonolith.Shared.Application.Commands;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using Assembly = System.Reflection.Assembly;

namespace ModularMonolith.Shared.ArchitectureTests.Application;

public class ApplicationTests
{
    private static readonly Architecture Architecture = new ArchLoader().LoadAssemblies
    (
        Assembly.Load("ModularMonolith.CategoryManagement.Application"),
        Assembly.Load("ModularMonolith.Identity.Application")
    ).Build();

    [Fact]
    public void ShouldAllCommandHandlersBeInternalAndSealed()
    {
        var rule = Classes()
            .That()
            .AreAssignableTo(typeof(ICommandHandler<>), typeof(ICommandHandler<,>))
            .Should()
            .BeInternal()
            .AndShould()
            .BeSealed()
            .AndShould()
            .HaveNameEndingWith("Handler");

        Architecture.CheckRule(rule);
    }

    [Fact]
    public void ShouldAllCommandValidatorsBeInternalAndSealed()
    {
        var rule = Classes()
            .That()
            .AreAssignableTo(typeof(AbstractValidator<>))
            .Should()
            .BeInternal()
            .AndShould()
            .BeSealed()
            .AndShould()
            .HaveNameEndingWith("Validator");

        Architecture.CheckRule(rule);
    }
}
