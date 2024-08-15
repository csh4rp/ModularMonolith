using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using ModularMonolith.Identity.Contracts.Account.PasswordReset;
using ModularMonolith.Identity.Domain.Users;
using ModularMonolith.Shared.Application.Commands;
using ModularMonolith.Shared.Application.Exceptions;
using ModularMonolith.Shared.Contracts.Errors;
using ModularMonolith.Shared.Messaging;

namespace ModularMonolith.Identity.Application.Account.PasswordReset;

internal sealed class ResetPasswordCommandHandler : ICommandHandler<ResetPasswordCommand>
{
    private readonly UserManager<User> _userManager;
    private readonly IMessageBus _messageBus;
    private readonly ILogger<ResetPasswordCommandHandler> _logger;

    public ResetPasswordCommandHandler(UserManager<User> userManager, IMessageBus messageBus,
        ILogger<ResetPasswordCommandHandler> logger)
    {
        _userManager = userManager;
        _messageBus = messageBus;
        _logger = logger;
    }

    public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString())
                   ?? throw new ValidationException(MemberError.InvalidValue(nameof(request.UserId)));

        var result = await _userManager.ResetPasswordAsync(user, request.ResetPasswordToken, request.NewPassword);
        if (result.Succeeded)
        {
            await _messageBus.PublishAsync(new PasswordResetEvent(user.Id), cancellationToken);
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
