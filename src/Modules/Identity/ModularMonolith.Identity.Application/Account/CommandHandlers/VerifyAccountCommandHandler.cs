using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using ModularMonolith.Identity.Contracts.Account.Commands;
using ModularMonolith.Identity.Domain.Common.Entities;
using ModularMonolith.Identity.Domain.Common.Events;
using ModularMonolith.Shared.Application.Commands;
using ModularMonolith.Shared.Application.Events;
using ModularMonolith.Shared.Application.Exceptions;
using ModularMonolith.Shared.Contracts.Errors;

namespace ModularMonolith.Identity.Application.Account.CommandHandlers;

internal sealed class VerifyAccountCommandHandler : ICommandHandler<VerifyAccountCommand>
{
    private readonly UserManager<User> _userManager;
    private readonly IEventBus _eventBus;
    private readonly ILogger<VerifyAccountCommandHandler> _logger;

    public VerifyAccountCommandHandler(UserManager<User> userManager, IEventBus eventBus,
        ILogger<VerifyAccountCommandHandler> logger)
    {
        _userManager = userManager;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task Handle(VerifyAccountCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null)
        {
            _logger.LogWarning("An attempt was made to verify an account with ID: {UserId} that does not exist",
                request.UserId);

            throw new ValidationException(MemberError.InvalidValue(nameof(request.UserId)));
        }

        var result = await _userManager.ConfirmEmailAsync(user, request.VerificationToken);
        if (!result.Succeeded)
        {
            _logger.LogWarning("An attempt was made to verify an account with ID: {UserId} with invalid token",
                user.Id);

            throw new ValidationException(MemberError.InvalidValue(nameof(request.VerificationToken)));
        }

        await _eventBus.PublishAsync(new AccountVerified(user.Id), cancellationToken);
    }
}
