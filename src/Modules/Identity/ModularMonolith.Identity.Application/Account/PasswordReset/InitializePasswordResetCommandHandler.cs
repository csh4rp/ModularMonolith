using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using ModularMonolith.Identity.Contracts.Account.PasswordReset;
using ModularMonolith.Identity.Domain.Users;
using ModularMonolith.Shared.Application.Commands;
using ModularMonolith.Shared.Messaging;

namespace ModularMonolith.Identity.Application.Account.PasswordReset;

internal sealed class InitializePasswordResetCommandHandler : ICommandHandler<InitializePasswordResetCommand>
{
    private readonly UserManager<User> _userManager;
    private readonly IMessageBus _messageBus;
    private readonly ILogger<InitializePasswordResetCommandHandler> _logger;

    public InitializePasswordResetCommandHandler(UserManager<User> userManager,
        IMessageBus messageBus,
        ILogger<InitializePasswordResetCommandHandler> logger)
    {
        _userManager = userManager;
        _messageBus = messageBus;
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

        await _messageBus.PublishAsync(new PasswordResetInitializedEvent(user.Id), cancellationToken);
    }
}
