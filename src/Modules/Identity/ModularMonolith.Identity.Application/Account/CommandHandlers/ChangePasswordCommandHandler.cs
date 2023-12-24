using Microsoft.AspNetCore.Identity;
using ModularMonolith.Identity.Contracts.Account.Commands;
using ModularMonolith.Identity.Domain.Users.Entities;
using ModularMonolith.Identity.Domain.Users.Events;
using ModularMonolith.Shared.Application.Commands;
using ModularMonolith.Shared.Application.Events;
using ModularMonolith.Shared.Application.Identity;

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

    public async Task Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var userId = _identityContextAccessor.Context!.UserId;

        var user = await _userManager.FindByIdAsync(userId.ToString());

        var result = await _userManager.ChangePasswordAsync(user!, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            return;
        }

        await _eventBus.PublishAsync(new PasswordChanged(userId), cancellationToken);
    }
}
