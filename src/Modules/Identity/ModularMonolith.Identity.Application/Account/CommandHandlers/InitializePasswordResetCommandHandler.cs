using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using ModularMonolith.Identity.Contracts.Account.Commands;
using ModularMonolith.Identity.Domain.Common.Entities;
using ModularMonolith.Identity.Domain.Common.Events;
using ModularMonolith.Shared.Application.Commands;
using ModularMonolith.Shared.Application.Events;
namespace ModularMonolith.Identity.Application.Account.CommandHandlers;

internal sealed class InitializePasswordResetCommandHandler : ICommandHandler<InitializePasswordResetCommand>
{
    private readonly UserManager<User> _userManager;
    private readonly IEventBus _eventBus;
    private readonly ILogger<InitializePasswordResetCommandHandler> _logger;

    public InitializePasswordResetCommandHandler(UserManager<User> userManager,
        IEventBus eventBus,
        ILogger<InitializePasswordResetCommandHandler> logger)
    {
        _userManager = userManager;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task Handle(InitializePasswordResetCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null)
        {
            _logger.LogWarning("Password reset initialized for not existing user");
            return;
        }

        await _eventBus.PublishAsync(new PasswordResetInitialized(user.Id), cancellationToken);
    }
}
