using Microsoft.OpenApi.Models;
using ModularMonolith.Identity.RestApi.Account;
using ModularMonolith.Identity.Infrastructure;
using ModularMonolith.Shared.RestApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace ModularMonolith.Identity.RestApi;

public sealed class IdentityWebModule : IdentityModule, IWebAppModule
{
    public WebApplication RegisterEndpoints(WebApplication app)
    {
        app.UseAccountEndpoints();
        return app;
    }
    public void SwaggerGenAction(SwaggerGenOptions options) =>
        options.SwaggerDoc("identity-v1", new OpenApiInfo { Version = "v1.0" });

    public void SwaggerUIAction(SwaggerUIOptions options) =>
        options.SwaggerEndpoint("/swagger/identity-v1/swagger.json", "Identity v1.0");
}
