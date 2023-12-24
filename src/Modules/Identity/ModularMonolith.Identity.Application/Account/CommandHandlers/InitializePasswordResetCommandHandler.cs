using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModularMonolith.Identity.Contracts.Account.Commands;
using ModularMonolith.Identity.Domain.Users.Entities;
using ModularMonolith.Identity.Domain.Users.Events;
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
        var normalizedEmail = _userManager.NormalizeEmail(request.Email);

        var userId = await _userManager.Users
            .Where(u => u.NormalizedEmail == normalizedEmail)
            .Select(u => u.Id)
            .SingleOrDefaultAsync(cancellationToken);
        
        if (userId == default)
        {
            _logger.LogWarning("Password reset initialized for not existing user");
            
            return;
        }

        await _eventBus.PublishAsync(new PasswordResetInitialized(userId), cancellationToken);
    }
}
