using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ModularMonolith.Modules.Identity.BusinessLogic.Account.Exceptions;
using ModularMonolith.Modules.Identity.Contracts.Account.Commands;
using ModularMonolith.Modules.Identity.Domain.Users;
using ModularMonolith.Modules.Identity.Domain.Users.Entities;
using ModularMonolith.Modules.Identity.Domain.Users.Events;
using ModularMonolith.Shared.BusinessLogic.Commands;
using ModularMonolith.Shared.BusinessLogic.Events;
using ModularMonolith.Shared.BusinessLogic.Exceptions;
using ModularMonolith.Shared.Contracts.Errors;

namespace ModularMonolith.Modules.Identity.BusinessLogic.Account.CommandHandlers;

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
        var normalizedEmail = _userManager.NormalizeEmail(request.Email);

        var emailExists = await _userManager.Users.AnyAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken);
        if (emailExists)
        {
            throw new ValidationException(PropertyError.NotUnique(nameof(RegisterCommand.Email), request.Email));
        }
        
        var user = new User { UserName = request.Email, Email = request.Email };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            throw new UserRegistrationException(result.Errors);
        }

        await _eventBus.PublishAsync(new UserRegistered(user.Id, user.Email), cancellationToken);
    }
}
