using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using ModularMonolith.Shared.Contracts.Errors;

namespace ModularMonolith.Shared.TestUtils.Assertions;

public class ErrorAssertions : ReferenceTypeAssertions<Error?, ErrorAssertions>
{
    public ErrorAssertions(Error? subject) : base(subject)
    {
    }

    protected override string Identifier => "Error";
    
    public AndConstraint<ErrorAssertions<T>> HaveType<T>(string because = "", params object[] becauseArgs) where T : Error
    {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(Subject is T)
            .FailWith("Expected {context:Error} to be of type {0}{reason}, but it was {1}", typeof(T), Subject!.GetType());

        return new AndConstraint<ErrorAssertions<T>>(new ErrorAssertions<T>((T)Subject));
    }
    
    public AndConstraint<MemberErrorAssertions> BeMemberError(string because = "", params object[] becauseArgs)
    {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(Subject is MemberError)
            .FailWith("Expected {context:Error} to be of type {0}{reason}, but it was {1}", typeof(MemberError), Subject!.GetType());

        return new AndConstraint<MemberErrorAssertions>(new MemberErrorAssertions((MemberError)Subject));
    }
    
    public AndConstraint<ConflictErrorAssertions> BeConflictError(string because = "", params object[] becauseArgs)
    {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(Subject is ConflictError)
            .FailWith("Expected {context:Error} to be of type {0}{reason}, but it was {1}", typeof(ConflictError), Subject!.GetType());

        return new AndConstraint<ConflictErrorAssertions>(new ConflictErrorAssertions((ConflictError)Subject));
    }
}

public class MemberErrorAssertions : ReferenceTypeAssertions<MemberError, MemberErrorAssertions>
{
    public MemberErrorAssertions(MemberError subject) : base(subject)
    {
    }

    protected override string Identifier => "Error";
    
    public AndConstraint<MemberErrorAssertions> HaveCode(string code, string because = "", params object[] becauseArgs)
    {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(Subject.Code == code)
            .FailWith("Expected {context:Error} to have code {0}{reason}, but it was {1}", code, Subject.Code);

        return new AndConstraint<MemberErrorAssertions>(this);
    }
    
    public AndConstraint<MemberErrorAssertions> HaveTarget(string target, string because = "", params object[] becauseArgs)
    {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(Subject.Target == target)
            .FailWith("Expected {context:Error} to have code {0}{reason}, but it was {1}", target, Subject.Target);

        return new AndConstraint<MemberErrorAssertions>(this);
    }
}

public class ConflictErrorAssertions : ReferenceTypeAssertions<ConflictError, ConflictErrorAssertions>
{
    public ConflictErrorAssertions(ConflictError subject) : base(subject)
    {
    }

    protected override string Identifier => "Error";
    
    public AndConstraint<ConflictErrorAssertions> HaveTarget(string target, string because = "", params object[] becauseArgs)
    {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(Subject.Target == target)
            .FailWith("Expected {context:Error} to have code {0}{reason}, but it was {1}", target, Subject.Target);

        return new AndConstraint<ConflictErrorAssertions>(this);
    }
}

public class ErrorAssertions<T> : ReferenceTypeAssertions<T, ErrorAssertions<T>>
{
    public ErrorAssertions(T subject) : base(subject)
    {
    }

    protected override string Identifier => "Error";
}
