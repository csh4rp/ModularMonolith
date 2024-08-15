using ModularMonolith.Shared.Contracts.Errors;

namespace ModularMonolith.Shared.Application.Exceptions;

public sealed class ValidationException : ApplicationLogicException
{
    public ValidationException(params MemberError[] propertyErrors) :
        base("One or more validation errors occurred.") => Errors = propertyErrors;

    public override string Code => "VALIDATION_ERROR";

    public IReadOnlyList<MemberError> Errors { get; }
}
