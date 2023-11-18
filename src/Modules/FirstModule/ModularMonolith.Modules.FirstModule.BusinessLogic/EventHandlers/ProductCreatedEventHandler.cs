using MassTransit;
using ModularMonolith.Modules.FirstModule.Domain.Events;

namespace ModularMonolith.Modules.FirstModule.BusinessLogic.EventHandlers;

public class ProductCreatedEventHandler : IConsumer<ProductCreated>
{
    public Task Consume(ConsumeContext<ProductCreated> context)
    {
        return Task.CompletedTask;
    }
}
