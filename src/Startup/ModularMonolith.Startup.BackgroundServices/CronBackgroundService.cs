using System.Diagnostics;
using MassTransit;
using ModularMonolith.Identity.Contracts.Account.Anonymisation;
using ModularMonolith.Shared.Messaging.MassTransit;

namespace ModularMonolith.Startup.BackgroundServices;

public class CronBackgroundService : BackgroundService
{
    private readonly IBus _bus;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public CronBackgroundService(IBus bus, IServiceScopeFactory serviceScopeFactory)
    {
        _bus = bus;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();

        var bus = scope.ServiceProvider.GetRequiredService<IRequestClient<AnonymiseAccountsJob>>();

        // await bus.ScheduleRecurringSend(new Uri("queue:AnonymiseAccountsJob"),
        //     new CronSchedule("0 0/1 * 1/1 * ? *"), new AnonymiseAccountsJob(), stoppingToken);

        var uri = new Uri("queue:quartz");
        var endpoint = await _bus.GetSendEndpoint(uri);

        var msg = await endpoint.ScheduleRecurringSend<AnonymiseAccountsJob>(new Uri("queue:AnonymiseAccountsJobConsumer"),
            new CronSchedule("* * * * * ?"),
            new AnonymiseAccountsJob(),
            stoppingToken);

        var en = await _bus.GetPublishSendEndpoint<AnonymiseAccountsJob>();
        await en.Send(new AnonymiseAccountsJob(), stoppingToken);



        Debug.Assert(msg is not null);
    }
}
