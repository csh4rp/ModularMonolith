using FluentValidation;
using ModularMonolith.Bootstrapper;
using ModularMonolith.Shared.BusinessLogic;
using ModularMonolith.Shared.Infrastructure.DataAccess;
using ModularMonolith.Shared.Infrastructure.Events;
using ModularMonolith.Shared.Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);

var modules = builder.Configuration.GetEnabledModules().ToList();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var assemblies = modules.SelectMany(m => m.Assemblies).ToArray();

builder.Services.AddMediator(assemblies);

builder.Services.AddValidatorsFromAssemblies(assemblies, includeInternalTypes: true);

builder.Services.AddEvents(e =>
{
    e.Assemblies = [.. assemblies];
});

builder.Services.AddIdentity()
    .AddSingleton(TimeProvider.System);


builder.Services.AddDataAccess(c =>
{
    c.ConnectionString = builder.Configuration.GetConnectionString("Database")!;
});

foreach (var appModule in modules)
{
    appModule.RegisterServices(builder.Services);
}

var app = builder.Build();

foreach (var appModule in modules)
{
    appModule.RegisterEndpoints(app);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

await app.RunAsync();
