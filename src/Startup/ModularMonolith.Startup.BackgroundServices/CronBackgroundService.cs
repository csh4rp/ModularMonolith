using MassTransit;
using ModularMonolith.Identity.Contracts.Account.Anonymisation;
using ModularMonolith.Shared.Messaging.MassTransit;

namespace ModularMonolith.Startup.BackgroundServices;

public class CronBackgroundService : BackgroundService
{
    private readonly IBus _bus;

    public CronBackgroundService(IBus bus) => _bus = bus;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var endpoint = await _bus.GetSendEndpoint(MessagingConstants.ScheduleQueueUri);

        _ = await endpoint.ScheduleRecurringSend<AnonymiseAccountsJob>(new Uri("queue:AnonymiseAccountsJobConsumer"),
            new CronSchedule("*/5 * * * * ?"),
            new AnonymiseAccountsJob(),
            stoppingToken);
    }
}
