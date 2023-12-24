using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModularMonolith.Modules.Identity.Contracts.Account.Commands;
using ModularMonolith.Modules.Identity.Domain.Users.Entities;
using ModularMonolith.Modules.Identity.Domain.Users.Events;
using ModularMonolith.Shared.BusinessLogic.Commands;
using ModularMonolith.Shared.BusinessLogic.Events;

namespace ModularMonolith.Modules.Identity.BusinessLogic.Account.CommandHandlers;

internal sealed class ResetPasswordCommandHandler : ICommandHandler<ResetPasswordCommand>
{
    private readonly UserManager<User> _userManager;
    private readonly IEventBus _eventBus;
    private readonly ILogger<ResetPasswordCommandHandler> _logger;
    
    public ResetPasswordCommandHandler(UserManager<User> userManager,
        IEventBus eventBus, 
        ILogger<ResetPasswordCommandHandler> logger)
    {
        _userManager = userManager;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
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

        await _eventBus.PublishAsync(new ResetPasswordInitialized(userId), cancellationToken);
    }
}
