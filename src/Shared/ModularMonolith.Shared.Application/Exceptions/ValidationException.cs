using ModularMonolith.Shared.Contracts.Errors;

namespace ModularMonolith.Shared.Application.Exceptions;

public sealed class ValidationException : AppException
{
    public ValidationException(params PropertyError[] propertyErrors) :
        base("One or more validation errors occurred.") => Errors = propertyErrors;

    public override string Code => "VALIDATION_ERROR";

    public IReadOnlyList<PropertyError> Errors { get; }
}
