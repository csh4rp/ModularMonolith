using MassTransit;
using MediatR;
using ModularMonolith.CategoryManagement.Domain.Categories;

namespace ModularMonolith.CategoryManagement.Infrastructure.Categories.Messaging;

public class CategoryCreatedConsumer : IConsumer<CategoryCreatedEvent>
{
    private readonly IMediator _mediator;

    public CategoryCreatedConsumer(IMediator mediator) => _mediator = mediator;

    public async Task Consume(ConsumeContext<CategoryCreatedEvent> context)
    {
        await _mediator.Publish(context.Message, context.CancellationToken);
    }
}
