namespace ModularMonolith.Shared.Api;

public static class WebApplicationException
{
    public static WebApplication PreparePipeline(this WebApplication app)
    {
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
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        
        return app;
    }
}
