using System.Diagnostics;
using MassTransit;
using ModularMonolith.Identity.Contracts.Account.Anonymisation;
using ModularMonolith.Shared.Messaging.MassTransit;

namespace ModularMonolith.Startup.BackgroundServices;

public class CronBackgroundService : BackgroundService
{
    private readonly IBus _bus;

    public CronBackgroundService(IBus bus)
    {
        _bus = bus;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var uri = new Uri("queue:scheduler");
        var endpoint = await _bus.GetSendEndpoint(uri);

        var msg = await endpoint.ScheduleRecurringSend(uri,
            new CronSchedule("* * * * *"),
            new AnonymiseAccountsJob(),
            stoppingToken);

        Debug.Assert(msg is not null);
    }
}
