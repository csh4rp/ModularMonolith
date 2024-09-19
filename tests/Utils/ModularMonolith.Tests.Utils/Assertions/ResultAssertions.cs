using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.Tests.Utils.Assertions;

public class ResultAssertions : ReferenceTypeAssertions<Result, ResultAssertions>
{
    public ResultAssertions(Result subject) : base(subject)
    {
    }

    protected override string Identifier => "Result";

    public AndConstraint<ResultAssertions> BeSuccessful(string because = "", params object[] becauseArgs)
    {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(Subject.IsSuccessful)
            .FailWith("Expected {context:Result} to be successful{reason}, but it failed with {0}", Subject.Error);

        return new AndConstraint<ResultAssertions>(this);
    }

    public AndConstraint<ResultAssertions> NotBeSuccessful(string because = "", params object[] becauseArgs)
    {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(!Subject.IsSuccessful)
            .FailWith("Expected {context:Result} to be successful{reason}, but it failed with {0}", Subject.Error);

        return new AndConstraint<ResultAssertions>(this);
    }
}

public class ResultAssertions<T> : ReferenceTypeAssertions<Result<T>, ResultAssertions<T>>
{
    public ResultAssertions(Result<T> subject) : base(subject)
    {
    }

    protected override string Identifier => "Result";

    public AndConstraint<ResultAssertions<T>> BeSuccessful(string because = "", params object[] becauseArgs)
    {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(Subject.IsSuccessful)
            .FailWith("Expected {context:Result} to be successful{reason}, but it failed with {0}", Subject.Error);

        return new AndConstraint<ResultAssertions<T>>(this);
    }

    public AndConstraint<ResultAssertions<T>> NotBeSuccessful(string because = "", params object[] becauseArgs)
    {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(!Subject.IsSuccessful)
            .FailWith("Expected {context:Result} to be successful{reason}, but it failed with {0}", Subject.Error);

        return new AndConstraint<ResultAssertions<T>>(this);
    }
}
