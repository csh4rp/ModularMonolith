using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.Shared.Api.CustomResults;

public abstract class PaginatedResult
{
    public static PaginatedResult<T> From<T>(IPaginatedResponse<T> response) =>
        new(response.Items, response.TotalLength);
}

public class PaginatedResult<T> : IResult
{
    private readonly IReadOnlyCollection<T> _items;
    private readonly int _totalLength;

    public PaginatedResult(IReadOnlyList<T> items, int totalLength)
    {
        _items = items;
        _totalLength = totalLength;
    }

    public Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.Headers.Append("X-Total-Length", _totalLength.ToString());
        return httpContext.Response.WriteAsJsonAsync(_items);
    }
}
