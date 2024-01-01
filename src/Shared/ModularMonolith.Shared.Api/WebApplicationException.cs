using Asp.Versioning;

namespace ModularMonolith.Shared.Api;

public static class WebApplicationException
{
    public static WebApplication PreparePipeline(this WebApplication app)
    {
        app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .ReportApiVersions()
            .Build();
        
        app.UseExceptionHandler(o =>
        {
            o.Use(async (c, d) =>
            {
                await d();
            });
        });

        var modules = app.Services.GetServices<AppModule>().ToList();
     
        foreach (var module in modules)
        {
            module.RegisterEndpoints(app);
        }
        
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
        
        return app;
    }
}
