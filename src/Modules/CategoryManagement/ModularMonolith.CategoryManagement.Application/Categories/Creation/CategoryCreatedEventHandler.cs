using Microsoft.Extensions.Logging;
using ModularMonolith.CategoryManagement.Domain.Categories;
using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.CategoryManagement.Application.Categories.Creation;

internal sealed class CategoryCreatedEventHandler : IEventHandler<CategoryCreatedEvent>
{
    private readonly ILogger<CategoryCreatedEventHandler> _logger;

    public CategoryCreatedEventHandler(ILogger<CategoryCreatedEventHandler> logger) => _logger = logger;

    public Task Handle(CategoryCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("CategoryCreated event was handled");

        return Task.CompletedTask;
    }
}
