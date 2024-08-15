namespace ModularMonolith.Shared.Tracing;

public interface IOperationContextAccessor
{
    IOperationContext? OperationContext { get; }
}
