using System.Diagnostics.CodeAnalysis;
using ModularMonolith.Shared.Contracts.Errors;

namespace ModularMonolith.Shared.Contracts;

public readonly record struct Result
{
    public static readonly Result Successful = new();

    public Result()
    {
    }

    private Result(Error error) => Error = error;

    public Error? Error { get; private init; }

    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccessful => Error is null;

    public static implicit operator Error?(Result result) => result.Error;

    public static implicit operator Result(Error error) => new(error);

    public static Result Failure(Error error) => new(error);
}

public readonly record struct Result<T>
{
    public Result(T value) => Value = value;

    public Result(Error error) => Error = error;

    public T? Value { get; private init; }

    public Error? Error { get; private init; }

    [MemberNotNullWhen(false, nameof(Error))]
    [MemberNotNullWhen(true, nameof(Value))]
    public bool IsSuccessful => Error is null;

    public static implicit operator Error?(Result<T> result) => result.Error;

    public static implicit operator Result<T>(Error error) => new(error);

    public static implicit operator Result<T>(T value) => new(value);

    public static implicit operator T?(Result<T> result) => result.Value;
}
