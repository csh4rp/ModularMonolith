namespace ModularMonolith.Identity.Domain.Users;

public sealed class User
{
    private User()
    {
        Email = default!;
        NormalizedEmail = default!;
        UserName = default!;
        NormalizedUserName = default!;
        PasswordHash = default!;
        SecurityStamp = default!;
        ConcurrencyStamp = default!;
    }

    public User(string email)
    {
        Email = email;
        NormalizedEmail = email.ToUpperInvariant();
        UserName = email;
        NormalizedUserName = email.ToUpperInvariant();
        PasswordHash = new Random().Next().ToString();
        SecurityStamp = Guid.NewGuid().ToString();
        ConcurrencyStamp = Guid.NewGuid().ToString();
    }

    public Guid Id { get; set; }

    public string UserName { get; set; }

    public string NormalizedUserName { get; set; }

    public string Email { get; set; }

    public string NormalizedEmail { get; set; }

    public bool EmailConfirmed { get; set; }

    public string PasswordHash { get; set; }

    public string SecurityStamp { get; set; }

    public string ConcurrencyStamp { get; set; }

    public string? PhoneNumber { get; set; }

    public bool PhoneNumberConfirmed { get; set; }

    public bool TwoFactorEnabled { get; set; }

    public DateTimeOffset? LockoutEnd { get; set; }

    public bool LockoutEnabled { get; set; }

    public int AccessFailedCount { get; set; }
}
