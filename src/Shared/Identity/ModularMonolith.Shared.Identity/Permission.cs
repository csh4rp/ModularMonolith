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

        var valuesIndex = 0;
        var permissionsIndex = 0;

        while (permissionsIndex < permissionParts.Length)
        {
            var currentPermission = permissionParts[permissionsIndex];
            var currentValue = valueParts[valuesIndex];

            if (currentPermission.Equals(currentValue))
            {
                valuesIndex++;
                permissionsIndex++;
                continue;
            }

            if (currentValue.Equals(Wildcard))
            {
                if (valuesIndex == valueParts.Length - 1)
                {
                    break;
                }

                valuesIndex++;
                currentValue = valueParts[valuesIndex];
                while (permissionsIndex < permissionParts.Length)
                {
                    currentPermission = permissionParts[permissionsIndex];
                    if (currentValue.Equals(currentPermission))
                    {
                        break;
                    }

                    permissionsIndex++;
                }

                continue;
            }

            return false;
        }

        return true;
    }
}
