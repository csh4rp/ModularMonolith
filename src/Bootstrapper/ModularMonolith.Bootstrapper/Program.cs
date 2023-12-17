using ModularMonolith.Modules.FirstModule.Api;
using ModularMonolith.Shared.Infrastructure.DataAccess;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.addev
builder.Services.AddDataAccess(c =>
{
    c.ConnectionString = builder.Configuration.GetConnectionString("Database")!;
});

builder.Services.AddFirstModule();


var app = builder.Build();

app.UseFirstModule();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

await app.RunAsync();
