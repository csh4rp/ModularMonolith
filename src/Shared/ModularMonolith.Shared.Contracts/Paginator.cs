namespace ModularMonolith.Shared.Contracts;

public class Paginator<T>
{
    private readonly string? _fieldName;
    private readonly bool? _isAscending;
    
    public Paginator(int? skip, int? take, string? orderBy)
    {
        Skip = skip;
        Take = take;
        OrderBy = orderBy;

        (_fieldName, _isAscending) = GetOrderInfo(orderBy);
    }
    
    public int? Skip { get; }
    
    public int? Take { get; }
    
    public string? OrderBy { get; }

    public bool HasOrderByExpression => !string.IsNullOrEmpty(OrderBy);

    public string FieldName => _fieldName 
                               ?? throw new InvalidOperationException("Paginator does not contain OrderBy expression");

    public bool IsAscending => _isAscending 
                                ?? throw new InvalidOperationException("Paginator does not contain OrderBy expression");

    private static (string? FieldName, bool? IsDescending) GetOrderInfo(string? orderBy)
    {
        if (string.IsNullOrEmpty(orderBy))
        {
            return default;
        }

        var index = orderBy.IndexOf(':');

        if (index == -1)
        {
            index = orderBy.Length - 1;
        }

        var span = orderBy.AsSpan();
        
        var fieldName = span[..index];
        var direction = span[index..];

        var isDescending = direction is "desc";

        return (fieldName.ToString(), isDescending);
    }

}
