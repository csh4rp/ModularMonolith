using Microsoft.AspNetCore.Identity;
using ModularMonolith.Identity.Contracts.Account.PasswordChange;
using ModularMonolith.Identity.Domain.Users;
using ModularMonolith.Shared.Application.Commands;
using ModularMonolith.Shared.Application.Exceptions;
using ModularMonolith.Shared.Contracts.Errors;
using ModularMonolith.Shared.Identity;
using ModularMonolith.Shared.Messaging;

namespace ModularMonolith.Identity.Application.Account.PasswordChange;

internal sealed class ChangePasswordCommandHandler : ICommandHandler<ChangePasswordCommand>
{
    private readonly UserManager<User> _userManager;
    private readonly IIdentityContextAccessor _identityContextAccessor;
    private readonly IMessageBus _messageBus;

    public ChangePasswordCommandHandler(UserManager<User> userManager,
        IIdentityContextAccessor identityContextAccessor,
        IMessageBus messageBus)
    {
        _userManager = userManager;
        _identityContextAccessor = identityContextAccessor;
        _messageBus = messageBus;
    }

    public async Task Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var subject = _identityContextAccessor.IdentityContext!.Subject;
        var user = await _userManager.FindByNameAsync(subject);
        var result = await _userManager.ChangePasswordAsync(user!, request.CurrentPassword, request.NewPassword);

        if (result.Succeeded)
        {
            await _messageBus.PublishAsync(new PasswordChangedEvent(user!.Id), cancellationToken);
            return;
        }

        if (result.Errors.Any(e => e.Code.Equals("PasswordMismatch", StringComparison.OrdinalIgnoreCase)))
        {
            throw new ValidationException(MemberError.InvalidValue(nameof(request.CurrentPassword)));
        }

        throw new ValidationException(MemberError.InvalidValue(nameof(request.NewPassword)));
    }
}
