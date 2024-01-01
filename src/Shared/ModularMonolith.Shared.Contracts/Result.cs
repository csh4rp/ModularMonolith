using System.Diagnostics.CodeAnalysis;
using ModularMonolith.Shared.Contracts.Errors;

namespace ModularMonolith.Shared.Contracts;

public readonly record struct Result
{
    private enum ResultState : byte
    {
        Undefined,
        Success,
        Failure
    }

    public static readonly Result Successful = new(true);

    private readonly ResultState _resultState;

    public Result() => _resultState = ResultState.Undefined;

    private Result(bool isSuccessful) => _resultState = isSuccessful ? ResultState.Success : ResultState.Undefined;

    private Result(Error error)
    {
        Error = error;
        _resultState = ResultState.Failure;
    }

    public Error? Error { get; private init; }

    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccessful => _resultState == ResultState.Success;

    public static implicit operator Error?(Result result) => result.Error;

    public static implicit operator Result(Error error) => new(error);

    public static Result Failure(Error error) => new(error);
}

public readonly record struct Result<T>
{
    private enum ResultState : byte
    {
        Undefined,
        Success,
        Failure
    }

    private readonly ResultState _resultState;

    public Result() => _resultState = ResultState.Undefined;

    public Result(T value)
    {
        Value = value;
        _resultState = ResultState.Success;
    }

    public Result(Error error)
    {
        Error = error;
        _resultState = ResultState.Failure;
    }

    public T? Value { get; private init; }

    public Error? Error { get; private init; }

    [MemberNotNullWhen(false, nameof(Error))]
    [MemberNotNullWhen(true, nameof(Value))]
    public bool IsSuccessful => _resultState == ResultState.Success;

    public static implicit operator Error?(Result<T> result) => result.Error;

    public static implicit operator Result<T>(Error error) => new(error);

    public static implicit operator Result<T>(T value) => new(value);

    public static implicit operator T?(Result<T> result) => result.Value;
}
