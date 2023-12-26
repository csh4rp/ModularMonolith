using FluentValidation;
using ModularMonolith.Shared.Api.Exceptions;
using ModularMonolith.Shared.Application;
using ModularMonolith.Shared.Infrastructure.DataAccess;
using ModularMonolith.Shared.Infrastructure.Events;
using ModularMonolith.Shared.Infrastructure.Identity;

namespace ModularMonolith.CategoryManagement.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var module = new CategoryManagementModule();

        var assemblies = module.Assemblies;

        builder.Services.AddMediator([.. assemblies]);

        builder.Services.AddValidatorsFromAssemblies(assemblies, includeInternalTypes: true);

        builder.Services.AddEvents(e =>
        {
            e.Assemblies = [.. assemblies];
        });

        builder.Services
            .AddSingleton(TimeProvider.System)
            .AddIdentityServices()
            .AddHttpContextAccessor()
            .AddExceptionHandlers();

        builder.Services.AddDataAccess(c =>
        {
            c.ConnectionString = builder.Configuration.GetConnectionString("Database")!;
        });


        module.RegisterServices(builder.Services);
        
        var app = builder.Build();

        app.UseExceptionHandler(o =>
        {

        });
        
        module.RegisterEndpoints(app);


        app.Run();
    }
}
