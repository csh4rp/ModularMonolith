namespace ModularMonolith.Shared.Contracts.Errors;

public sealed class MemberError : Error
{
    public MemberError(string code, string message, string target) : base(code, message) => Target = target;

    public string Target { get; private set; }

    public static MemberError InvalidValue(string target) => 
        new(ErrorCodes.InvalidValue, "Value is invalid", target);

    public static MemberError Conflict(string target) => 
        new(ErrorCodes.Conflict, "Value is conflicting", target);
}
