﻿using ModularMonolith.Shared.Api;

namespace ModularMonolith.Identity.Api;

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
