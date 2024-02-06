using Bogus;
using ModularMonolith.Identity.Domain.Users;

namespace ModularMonolith.Identity.Api.IntegrationTests.Account.Fixtures;

public sealed class AccountFixture : Faker<User>, IClassFixture<AccountFixture>
{
    public const string Password = "123Pa$$word321";

    private const string PasswordHash =
        "AQAAAAIAAYagAAAAEMbO4U0+72DTJTrynS9mX+9SokWZbBi9opL+zLvvgauBIWs6QRwDpUFPaHdEe6e+Mw==";

    public AccountFixture() =>
        CustomInstantiator(f => new User(f.Person.Email) { Id = Guid.NewGuid(), PasswordHash = PasswordHash });

    public User AActiveUser() =>
        Clone()
            .RuleFor(x => x.Email, "mail@mail.com")
            .RuleFor(x => x.NormalizedEmail, (_, u) => u.Email.ToUpperInvariant())
            .RuleFor(x => x.UserName, (_, u) => u.Email)
            .RuleFor(x => x.NormalizedUserName, (_, u) => u.Email.ToUpperInvariant())
            .RuleFor(x => x.EmailConfirmed, true)
            .RuleFor(x => x.LockoutEnabled, false)
            .RuleFor(x => x.LockoutEnd, (DateTimeOffset?)null)
            .Generate();

    public User AUnverifiedUser() =>
        Clone()
            .RuleFor(x => x.Email, "mail@mail.com")
            .RuleFor(x => x.NormalizedEmail, (_, u) => u.Email.ToUpperInvariant())
            .RuleFor(x => x.UserName, (_, u) => u.Email)
            .RuleFor(x => x.NormalizedUserName, (_, u) => u.Email.ToUpperInvariant())
            .RuleFor(x => x.LockoutEnabled, false)
            .RuleFor(x => x.LockoutEnd, (DateTimeOffset?)null)
            .Generate();
}
