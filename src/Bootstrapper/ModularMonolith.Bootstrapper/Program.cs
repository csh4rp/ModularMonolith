using ModularMonolith.Shared.Api;

var builder = WebApplication.CreateBuilder(args)
    .RegisterModules();

var app = builder.Build()
    .PreparePipeline();

await app.RunAsync();
