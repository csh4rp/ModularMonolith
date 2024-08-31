namespace ModularMonolith.Shared.Identity;

public record Permission
{
    private const string Wildcard = "*";
    private const string Separator = "/";

    private readonly string _value;

    public Permission(string value) => _value = value;

    public bool GrantsAccessTo(string permission)
    {
        var permissionParts = permission.Split(Separator);
        var valueParts = _value.Split(Separator);

        var currentValue = valueParts[0];

        foreach (var permissionPart in permissionParts)
        {
            if (currentValue == Wildcard || currentValue == permissionPart)
            {
                continue;
            }


        }

        return true;
    }
}
