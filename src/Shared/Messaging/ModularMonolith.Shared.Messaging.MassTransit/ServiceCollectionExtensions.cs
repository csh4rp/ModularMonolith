﻿using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Shared.Messaging.MassTransit.Factories;

namespace ModularMonolith.Shared.Messaging.MassTransit;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMessageBus(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddScoped<EventLogEntryFactory>()
            .AddScoped<IMessageBus, MessageBus>();
    }
}