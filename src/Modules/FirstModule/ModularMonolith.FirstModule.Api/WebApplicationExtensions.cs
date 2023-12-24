using ModularMonolith.FirstModule.Api.Categories;

namespace ModularMonolith.FirstModule.Api;

public static class WebApplicationExtensions
{
    public static WebApplication UseFirstModule(this WebApplication app)
    {
        app.UseCategoryEndpoints();

        return app;
    }
}
