using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using ModularMonolith.Identity.Contracts.Account.PasswordReset;
using ModularMonolith.Identity.Domain.Users;
using ModularMonolith.Shared.Application.Commands;
using ModularMonolith.Shared.Application.Events;
using ModularMonolith.Shared.Application.Exceptions;
using ModularMonolith.Shared.Contracts.Errors;

namespace ModularMonolith.Identity.Application.Account.PasswordReset;

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
                   ?? throw new ValidationException(MemberError.InvalidValue(nameof(request.UserId)));

        var result = await _userManager.ResetPasswordAsync(user, request.ResetPasswordToken, request.NewPassword);
        if (result.Succeeded)
        {
            await _eventBus.PublishAsync(new PasswordResetEvent(user.Id), cancellationToken);
            return;
        }

        _logger.LogWarning("An attempt was made to reset password for user with ID: {UserId} using invalid token",
            user.Id);

        if (result.Errors.Any(e => e.Code.Equals("InvalidToken", StringComparison.OrdinalIgnoreCase)))
        {
            throw new ValidationException(MemberError.InvalidValue(nameof(request.ResetPasswordToken)));
        }

        throw new ValidationException(MemberError.InvalidValue(nameof(request.NewPassword)));
    }
}
