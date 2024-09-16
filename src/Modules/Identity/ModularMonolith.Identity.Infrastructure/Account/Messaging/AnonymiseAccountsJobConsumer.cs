using MassTransit;
using Microsoft.Extensions.Logging;
using ModularMonolith.Identity.Contracts.Account.Anonymisation;

namespace ModularMonolith.Identity.Infrastructure.Account.Messaging;

 public class AnonymiseAccountsJobConsumer : IJobConsumer<AnonymiseAccountsJob>
{
    private readonly ILogger<AnonymiseAccountsJobConsumer> _logger;

    public AnonymiseAccountsJobConsumer(ILogger<AnonymiseAccountsJobConsumer> logger)
    {
        _logger = logger;
    }

    public Task Run(JobContext<AnonymiseAccountsJob> context)
    {
        _logger.LogInformation("Running anonymistaion");
        return Task.CompletedTask;
    }

    public Task Consume(ConsumeContext<AnonymiseAccountsJob> context)
    {
        _logger.LogInformation("Running anonymistaion");
        return Task.CompletedTask;
    }
}
