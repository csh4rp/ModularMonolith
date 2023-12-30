using Microsoft.AspNetCore.Identity;
using ModularMonolith.Identity.Contracts.Account.Commands;
using ModularMonolith.Identity.Domain.Common.Entities;
using ModularMonolith.Identity.Domain.Common.Events;
using ModularMonolith.Shared.Application.Commands;
using ModularMonolith.Shared.Application.Events;
using ModularMonolith.Shared.Application.Identity;
using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.Identity.Application.Account.CommandHandlers;

internal sealed class ChangePasswordCommandHandler : ICommandHandler<ChangePasswordCommand, Result>
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
        if (!result.Succeeded)
        {
            return new Error("");
        }

        await _eventBus.PublishAsync(new PasswordChanged(userId), cancellationToken);

        return Result.Successful();
    }
}
