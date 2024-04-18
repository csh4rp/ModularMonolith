using ModularMonolith.Shared.RestApi;

namespace ModularMonolith.Identity.RestApi;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args)
            .RegisterModules();

        var app = builder.Build()
            .PreparePipeline();

        await app.RunAsync();
    }
}
