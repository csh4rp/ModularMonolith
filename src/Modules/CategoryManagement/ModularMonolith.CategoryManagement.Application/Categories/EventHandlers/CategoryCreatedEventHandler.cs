using MassTransit;
using ModularMonolith.CategoryManagement.Domain.Events;
using ModularMonolith.Shared.Domain.Attributes;

namespace ModularMonolith.CategoryManagement.Application.Categories.EventHandlers;

[EventConsumer("categories")]
public class CategoryCreatedEventHandler : IConsumer<CategoryCreated>
{
    public Task Consume(ConsumeContext<CategoryCreated> context)
    {
        return Task.CompletedTask;
    }
}
