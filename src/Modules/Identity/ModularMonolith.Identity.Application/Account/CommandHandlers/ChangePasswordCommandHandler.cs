using Microsoft.AspNetCore.Identity;
using ModularMonolith.Identity.Contracts.Account.Commands;
using ModularMonolith.Identity.Domain.Common.Entities;
using ModularMonolith.Identity.Domain.Common.Events;
using ModularMonolith.Shared.Application.Commands;
using ModularMonolith.Shared.Application.Events;
using ModularMonolith.Shared.Application.Identity;
using ModularMonolith.Shared.Contracts;
using ModularMonolith.Shared.Contracts.Errors;

namespace ModularMonolith.Identity.Application.Account.CommandHandlers;

internal sealed class ChangePasswordCommandHandler : ICommandHandler<ChangePasswordCommand>
{
    private readonly UserManager<User> _userManager;
    private readonly IIdentityContextAccessor _identityContextAccessor;
    private readonly IEventBus _eventBus;

    public ChangePasswordCommandHandler(UserManager<User> userManager, IIdentityContextAccessor identityContextAccessor,
        IEventBus eventBus)
    {
        _userManager = userManager;
        _identityContextAccessor = identityContextAccessor;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var userId = _identityContextAccessor.Context!.UserId;
        var user = await _userManager.FindByIdAsync(userId.ToString());
        var result = await _userManager.ChangePasswordAsync(user!, request.CurrentPassword, request.NewPassword);

        if (result.Succeeded)
        {
            await _eventBus.PublishAsync(new PasswordChanged(userId), cancellationToken);

            return Result.Successful;
        }

        if (result.Errors.Any(e => e.Code.Equals("PasswordMismatch", StringComparison.OrdinalIgnoreCase)))
        {
            return MemberError.InvalidValue(nameof(request.CurrentPassword));
        }

        return MemberError.InvalidValue(nameof(request.NewPassword));
    }
}
