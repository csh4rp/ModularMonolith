namespace ModularMonolith.Shared.Contracts.Errors;

public sealed class MemberError : Error
{
    public MemberError(string code, string message, string reference) : base(code, message) =>
        Reference = $"{char.ToLower(reference[0])}{reference[1..]}";

    public string Reference { get; private set; }

    public static MemberError InvalidValue(string reference) =>
        new(ErrorCodes.InvalidValue, "Value is invalid", reference);

    public static MemberError OutOfRange(string reference) =>
        new(ErrorCodes.OutOfRange, "Value is out of range", reference);

    public static MemberError Conflict(string reference) =>
        new(ErrorCodes.Conflict, "Conflict", reference);
}
