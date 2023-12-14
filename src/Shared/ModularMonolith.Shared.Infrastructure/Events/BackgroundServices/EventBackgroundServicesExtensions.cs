﻿using Microsoft.Extensions.DependencyInjection;

namespace ModularMonolith.Shared.Infrastructure.Events.BackgroundServices;

internal static class EventBackgroundServicesExtensions
{
    public static IServiceCollection AddEventBackgroundServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<EventNotificationFetchingBackgroundService>()
            .AddSingleton<EventPollingBackgroundService>()
            .AddSingleton<EventPublisherBackgroundService>();

        return serviceCollection;
    }
}
