using Microsoft.AspNetCore.Identity;
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

    public VerifyAccountCommandHandler(UserManager<User> userManager, IEventBus eventBus)
    {
        _userManager = userManager;
        _eventBus = eventBus;
    }

    public async Task Handle(VerifyAccountCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null)
        {
            throw new ValidationException(PropertyError.InvalidArgument(
                nameof(VerifyAccountCommand.UserId),
                request.UserId));
        }
        
        var result = await _userManager.ConfirmEmailAsync(user, request.VerificationToken);
        if (!result.Succeeded)
        {
            throw new ValidationException(PropertyError.InvalidArgument(
                nameof(VerifyAccountCommand.VerificationToken),
                request.VerificationToken));
        }

        await _eventBus.PublishAsync(new AccountVerified(user.Id), cancellationToken);
    }
}
