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

             // Current parts match
            if (currentPermission.Equals(currentValue))
            {
                valuesIndex++;
                permissionsIndex++;
                continue;
            }

            if (currentValue.Equals(Wildcard))
            {
                // If wildcard is last part - it's a march
                if (valuesIndex == valueParts.Length - 1)
                {
                    return true;
                }

                valuesIndex++;
                currentValue = valueParts[valuesIndex];

                // Try to find next matching part
                while (permissionsIndex < permissionParts.Length)
                {
                    currentPermission = permissionParts[permissionsIndex];

                    // Next matching part found
                    if (currentValue.Equals(currentPermission))
                    {
                        break;
                    }

                    // No matching part found
                    if (permissionsIndex == permissionParts.Length - 1)
                    {
                        return false;
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
