﻿using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using ModularMonolith.Modules.Identity.Contracts.Account.Commands;
using ModularMonolith.Modules.Identity.Domain.Users.Entities;
using ModularMonolith.Modules.Identity.Domain.Users.Events;
using ModularMonolith.Shared.BusinessLogic.Commands;
using ModularMonolith.Shared.BusinessLogic.Events;
using ModularMonolith.Shared.BusinessLogic.Exceptions;
using ModularMonolith.Shared.Contracts.Errors;

namespace ModularMonolith.Modules.Identity.BusinessLogic.Account.CommandHandlers;

internal sealed class VerifyAccountCommandHandler : ICommandHandler<VerifyAccountCommand>
{
    private readonly UserManager<User> _userManager;
    private readonly IEventBus _eventBus;
    private readonly ILogger<VerifyAccountCommandHandler> _logger;

    public VerifyAccountCommandHandler(UserManager<User> userManager, IEventBus eventBus, ILogger<VerifyAccountCommandHandler> logger)
    {
        _userManager = userManager;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task Handle(VerifyAccountCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString()) 
                   ?? throw new ValidationException(PropertyError.InvalidArgument(
                       nameof(VerifyAccountCommand.UserId), request.UserId));
        
        var result = await _userManager.ConfirmEmailAsync(user, request.VerificationToken);
        if (!result.Succeeded)
        {
            _logger.LogWarning("An attempt was made to verify an account with ID: {UserId} with invalid token",
                user.Id);
            
            throw new ValidationException(PropertyError.InvalidArgument(
                nameof(VerifyAccountCommand.VerificationToken), request.VerificationToken));
        }

        await _eventBus.PublishAsync(new AccountVerified(user.Id), cancellationToken);
    }
}
