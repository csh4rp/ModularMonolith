﻿using System.Diagnostics.CodeAnalysis;
using ModularMonolith.Shared.RestApi;

[assembly: ExcludeFromCodeCoverage]

namespace ModularMonolith.Startup.RestApi;

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
