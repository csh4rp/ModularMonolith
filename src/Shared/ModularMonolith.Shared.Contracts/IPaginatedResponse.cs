namespace ModularMonolith.Shared.Contracts;

public interface IPaginatedResponse<T>
{
    public IReadOnlyList<T> Items { get; init; }
    
    public int TotalLength { get; init; }
}
