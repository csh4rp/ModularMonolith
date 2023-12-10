using ModularMonolith.Modules.FirstModule.Api;
using ModularMonolith.Modules.FirstModule.Api.Categories;
using ModularMonolith.Shared.BusinessLogic;
using Extensions = ModularMonolith.Shared.BusinessLogic.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseFirstModule();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

await app.RunAsync();
