namespace ModularMonolith.Identity.Domain.Roles;

public sealed class Role
{
    private Role()
    {
        Name = default!;
        NormalizedName = default!;
        ConcurrencyStamp = default!;
    }

    public Role(string name)
    {
        Name = name;
        NormalizedName = name.ToUpperInvariant();
        ConcurrencyStamp = Guid.NewGuid().ToString();
    }

    public Guid Id { get; set; }

    public string Name { get; set; }

    public string NormalizedName { get; set; }

    public string ConcurrencyStamp { get; set; }
}
