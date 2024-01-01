using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using ModularMonolith.Identity.Contracts.Account.Commands;
using ModularMonolith.Identity.Domain.Common.Entities;
using ModularMonolith.Identity.Domain.Common.Events;
using ModularMonolith.Shared.Application.Commands;
using ModularMonolith.Shared.Application.Events;
using ModularMonolith.Shared.Contracts;
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

    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());

        if (user is null)
        {
            return MemberError.InvalidValue(nameof(request.UserId));
        }

        var result = await _userManager.ResetPasswordAsync(user, request.ResetPasswordToken, request.NewPassword);

        if (result.Succeeded)
        {
            await _eventBus.PublishAsync(new PasswordReset(user.Id), cancellationToken);

            return Result.Successful;
        }

        _logger.LogWarning("An attempt was made to reset password for user with ID: {UserId} using invalid token",
            user.Id);

        if (result.Errors.Any(e => e.Code.Equals("InvalidToken", StringComparison.OrdinalIgnoreCase)))
        {
            return MemberError.InvalidValue(nameof(request.ResetPasswordToken));
        }

        return MemberError.InvalidValue(nameof(request.NewPassword));
    }
}
