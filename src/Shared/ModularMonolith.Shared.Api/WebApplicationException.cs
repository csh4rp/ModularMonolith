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

        });

        var modules = app.Services.GetServices<AppModule>();
     
        foreach (var module in modules)
        {
            module.RegisterEndpoints(app);
        }
        
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger(o =>
            {
      
            });
            app.UseSwaggerUI(o =>
            {
                o.SwaggerEndpoint("/swagger/category-management-v1/swagger.json", "Category Management v1.0");
            });
        }
        
        return app;
    }
}
