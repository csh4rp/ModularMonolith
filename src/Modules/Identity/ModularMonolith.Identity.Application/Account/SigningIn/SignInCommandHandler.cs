using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ModularMonolith.Identity.Contracts.Account.SigningIn;
using ModularMonolith.Identity.Core.Options;
using ModularMonolith.Identity.Domain.Users;
using ModularMonolith.Shared.Application.Commands;
using ModularMonolith.Shared.Events;

namespace ModularMonolith.Identity.Application.Account.SigningIn;

internal sealed class SignInCommandHandler : ICommandHandler<SignInCommand, SignInResponse>
{
    private readonly UserManager<User> _userManager;
    private readonly IEventBus _eventBus;
    private readonly IOptions<AuthOptions> _options;
    private readonly TimeProvider _timeProvider;

    public SignInCommandHandler(UserManager<User> userManager,
        IEventBus eventBus,
        IOptions<AuthOptions> options,
        TimeProvider timeProvider)
    {
        _userManager = userManager;
        _eventBus = eventBus;
        _options = options;
        _timeProvider = timeProvider;
    }

    public async Task<SignInResponse> Handle(SignInCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return SignInResponse.InvalidCredentials();
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            await _eventBus.PublishAsync(new SignInFailedEvent(user.Id), cancellationToken);

            return SignInResponse.InvalidCredentials();
        }

        var options = _options.Value;

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, user.UserName!) };

        var expirationTime = _timeProvider.GetUtcNow().AddMinutes(options.ExpirationTimeInMinutes);

        var token = new JwtSecurityToken(options.Issuer,
            options.Audience,
            claims,
            expires: expirationTime.UtcDateTime,
            signingCredentials: credentials);

        var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

        await _eventBus.PublishAsync(new SignInSucceededEvent(user.Id), cancellationToken);

        return SignInResponse.Succeeded(tokenValue);
    }
}
