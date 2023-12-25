using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using ModularMonolith.Identity.Contracts.Account.Commands;
using ModularMonolith.Identity.Domain.Users.Entities;
using ModularMonolith.Identity.Domain.Users.Events;
using ModularMonolith.Shared.Application.Commands;
using ModularMonolith.Shared.Application.Events;
using ModularMonolith.Shared.Application.Exceptions;
using ModularMonolith.Shared.Contracts.Errors;

namespace ModularMonolith.Identity.Application.Account.CommandHandlers;

internal sealed class ResetPasswordCommandHandler : ICommandHandler<ResetPasswordCommand>
{
    private readonly UserManager<User> _userManager;
    private readonly IEventBus _eventBus;
    private readonly ILogger<ResetPasswordCommandHandler> _logger;

    public ResetPasswordCommandHandler(UserManager<User> userManager, IEventBus eventBus,
        ILogger<ResetPasswordCommandHandler> logger)
    {
        _userManager = userManager;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString())
                   ?? throw new ValidationException(PropertyError.InvalidArgument(
                       nameof(ResetPasswordCommand.UserId), request.UserId));

        var result = await _userManager.ResetPasswordAsync(user, request.ResetPasswordToken, request.Password);
        if (!result.Succeeded)
        {
            _logger.LogWarning("An attempt was made to reset password for user with ID: {UserId} using invalid token",
                user.Id);

            throw new ValidationException(PropertyError.InvalidArgument(
                nameof(ResetPasswordCommand.ResetPasswordToken), request.ResetPasswordToken));
        }

        await _eventBus.PublishAsync(new PasswordReset(user.Id), cancellationToken);
    }
}
