using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.Shared.Api.CustomResults;

public class CreatedAtResult : IResult
{
    private readonly CreatedResponse _response;

    public CreatedAtResult(CreatedResponse response)
    {
        _response = response;
    }

    public Task ExecuteAsync(HttpContext httpContext)
    {
        var url = httpContext.Request.Path.Add(new PathString($"/{_response.Id}"));
        
        httpContext.Response.Headers.Append("Location", url.ToString());
        httpContext.Response.StatusCode = StatusCodes.Status201Created;
        return httpContext.Response.WriteAsJsonAsync(_response);
    }
}
