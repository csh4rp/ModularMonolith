using MassTransit;
using Microsoft.Extensions.Logging;
using ModularMonolith.CategoryManagement.Domain.Categories;

namespace ModularMonolith.CategoryManagement.Application.Categories.Creation;

public sealed class CategoryCreatedConsumer : IConsumer<CategoryCreatedEvent>
{
    private readonly ILogger<CategoryCreatedConsumer> _logger;

    public CategoryCreatedConsumer(ILogger<CategoryCreatedConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<CategoryCreatedEvent> context)
    {
        _logger.LogInformation("Category with id: '{CategoryId}' was created", context.Message.CategoryId);
        return Task.CompletedTask;
    }
}
