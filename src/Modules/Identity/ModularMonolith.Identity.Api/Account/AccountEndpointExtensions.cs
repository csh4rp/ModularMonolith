namespace ModularMonolith.Identity.Api.Account;

internal static class AccountEndpointExtensions
{
    public static WebApplication UseAccountEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("account");
        
        group.MapPost()
        
        return app;
    }
}
