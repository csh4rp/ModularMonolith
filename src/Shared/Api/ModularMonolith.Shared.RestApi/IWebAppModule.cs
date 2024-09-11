using ModularMonolith.Shared.Infrastructure;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace ModularMonolith.Shared.RestApi;

public interface IWebAppModule : IAppModule
{
    public WebApplication RegisterEndpoints(WebApplication app);

    public void SwaggerGenAction(SwaggerGenOptions options);

    public void SwaggerUIAction(SwaggerUIOptions options);
}
