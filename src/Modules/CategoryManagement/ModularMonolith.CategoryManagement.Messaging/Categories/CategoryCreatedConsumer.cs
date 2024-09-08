using MassTransit;
using MassTransit.Mediator;
using ModularMonolith.CategoryManagement.Domain.Categories;

namespace ModularMonolith.CategoryManagement.Messaging.Categories;

public class CategoryCreatedConsumer : IConsumer<CategoryCreatedEvent>
{
    private readonly IMediator _mediator;

    public CategoryCreatedConsumer(IMediator mediator) => _mediator = mediator;

    public async Task Consume(ConsumeContext<CategoryCreatedEvent> context)
    {
        await _mediator.Publish(context.Message, context.CancellationToken);
    }
}
