using MassTransit;
using ModularMonolith.CategoryManagement.Domain.Categories;
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
