using MassTransit;
using Microsoft.Extensions.Logging;
using ModularMonolith.Identity.Contracts.Account.Anonymisation;

namespace ModularMonolith.Identity.Infrastructure.Account.Messaging;

 public sealed class AnonymiseAccountsJobConsumer : IJobConsumer<AnonymiseAccountsJob>
{
    private readonly ILogger<AnonymiseAccountsJobConsumer> _logger;

    public AnonymiseAccountsJobConsumer(ILogger<AnonymiseAccountsJobConsumer> logger)
    {
        _logger = logger;
    }

    public Task Run(JobContext<AnonymiseAccountsJob> context)
    {
        _logger.LogInformation("Running anonymisation");
        return Task.CompletedTask;
    }
}
