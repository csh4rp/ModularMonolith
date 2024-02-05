using MassTransit;
using ModularMonolith.CategoryManagement.Domain.Categories;
using ModularMonolith.Shared.Domain.Attributes;

namespace ModularMonolith.CategoryManagement.Application.Categories.Creation;

[EventConsumer("categories")]
public class CategoryCreatedEventHandler : IConsumer<CategoryCreatedEvent>
{
    public Task Consume(ConsumeContext<CategoryCreatedEvent> context)
    {
        return Task.CompletedTask;
    }
}
