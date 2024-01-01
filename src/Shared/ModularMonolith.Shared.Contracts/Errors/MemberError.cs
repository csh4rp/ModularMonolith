namespace ModularMonolith.Shared.Contracts.Errors;

public sealed class MemberError : Error
{
    public MemberError(string code, string message, string target) : base(code, message) => Target = $"{char.ToLower(target[0])}{target[1..]}";

    public string Target { get; private set; }

    public static MemberError InvalidValue(string target) => 
        new(ErrorCodes.InvalidValue, "Value is invalid", target);
}
