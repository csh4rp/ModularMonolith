using MassTransit;
using ModularMonolith.Identity.Domain.Users;

namespace ModularMonolith.Identity.Messaging.Account;

internal sealed class PasswordChangedConsumer : IConsumer<PasswordChangedEvent>
{
    public Task Consume(ConsumeContext<PasswordChangedEvent> context)
    {
        return Task.CompletedTask;
    }
}
