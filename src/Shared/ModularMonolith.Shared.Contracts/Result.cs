namespace ModularMonolith.Shared.Contracts;

public record struct Result
{
    private enum ResultState : byte
    {
        Success,
        Failure
    }

    private ResultState _resultState;

    public Result()
    {
        _resultState = ResultState.Success;
    }
    
    public Result(Error error)
    {
        Error = error;
        _resultState = ResultState.Failure;
    }
    
    public Error? Error { get; private set; }

    public bool IsSuccessful => _resultState == ResultState.Success;

    public static implicit operator Error?(Result result) => result.Error;
    
    public static implicit operator Result(Error error) => new(error);

    public static Result<T> Successful<T>(T value) => new(value);
    
    public static Result Successful() => new();
    
    public static Result<T> Failure<T>(Error error) => new(error);
    
    public static Result Failure(Error error) => new(error);
}

public record struct Result<T>
{
    private enum ResultState : byte
    {
        Success,
        Failure
    }

    private ResultState _resultState;

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
    
    public T? Value { get; private set; }
    
    public Error? Error { get; private set; }

    public bool IsSuccessful => _resultState == ResultState.Success;

    public static implicit operator Error?(Result<T> result) => result.Error;

    public static implicit operator Result<T>(Error error) => new(error);
    
    public static implicit operator T?(Result<T> result) => result.Value;
}
