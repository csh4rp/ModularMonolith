using ModularMonolith.Modules.FirstModule.Api.Categories;

namespace ModularMonolith.Modules.FirstModule.Api;

public static class WebApplicationExtensions
{
    public static WebApplication UseFirstModule(this WebApplication app)
    {
        app.UseCategoryEndpoints();
        
        return app;
    }
}
