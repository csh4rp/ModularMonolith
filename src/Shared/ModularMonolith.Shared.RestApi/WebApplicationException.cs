using Asp.Versioning;
using ModularMonolith.Shared.RestApi.Middlewares;

namespace ModularMonolith.Shared.RestApi;

public static class WebApplicationException
{
    public static WebApplication PreparePipeline(this WebApplication app)
    {
        app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .ReportApiVersions()
            .Build();

        var modules = app.Services.GetServices<AppModule>().ToList();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger(o =>
            {
            });
            app.UseSwaggerUI(options =>
            {
                foreach (var module in modules)
                {
                    module.SwaggerUIAction(options);
                }
            });
        }

        app.UseExceptionHandler(o =>
        {
            o.Use(async (c, d) =>
            {
                await d();
            });
        });

        app.UseRouting();


        app.UseAuthentication();
        app.UseAuthorization();

        app.UseMiddleware<IdentityMiddleware>();

        app.UseEndpoints(e =>
        {
        });

        foreach (var module in modules)
        {
            module.RegisterEndpoints(app);
        }

        return app;
    }
}
