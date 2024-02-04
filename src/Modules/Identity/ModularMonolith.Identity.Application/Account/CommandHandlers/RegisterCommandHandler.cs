using Microsoft.AspNetCore.Identity;
using ModularMonolith.Identity.Contracts.Account.Commands;
using ModularMonolith.Identity.Domain.Common.Entities;
using ModularMonolith.Identity.Domain.Common.Events;
using ModularMonolith.Shared.Application.Commands;
using ModularMonolith.Shared.Application.Events;
using ModularMonolith.Shared.Application.Exceptions;
using ModularMonolith.Shared.Contracts.Errors;

namespace ModularMonolith.Identity.Application.Account.CommandHandlers;

internal sealed class RegisterCommandHandler : ICommandHandler<RegisterCommand>
{
    private readonly UserManager<User> _userManager;
    private readonly IEventBus _eventBus;

    public RegisterCommandHandler(UserManager<User> userManager, IEventBus eventBus)
    {
        _userManager = userManager;
        _eventBus = eventBus;
    }

    public async Task Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var userWithGivenEmail = await _userManager.FindByEmailAsync(request.Email);
        if (userWithGivenEmail is not null)
        {
            throw new ValidationException(new MemberError(AccountErrorCodes.EmailConflict, 
                "Email was already used",
                nameof(request.Email)));
        }

        var user = new User(request.Email);

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            throw new ValidationException(new MemberError(AccountErrorCodes.PasswordNotMatchingPolicy, 
                "Password does not match current policy",
                nameof(request.Password)));
        }

        await _eventBus.PublishAsync(new UserRegistered(user.Id, user.Email), cancellationToken);
    }
}
