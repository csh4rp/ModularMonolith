namespace ModularMonolith.Shared.Identity.UnitTests;

public class PermissionTests
{
    [Theory]
    [MemberData(nameof(GetValidPermissions))]
    public void ShouldGrantAccess_WhenActualPermissionIsValid(string requiredPermission, string actualPermission)
    {
        var permission = new Permission(actualPermission);

        Assert.True(permission.GrantsAccessTo(requiredPermission));
    }

    [Theory]
    [MemberData(nameof(GetInvalidPermissions))]
    public void ShouldNotGrantAccess_WhenActualPermissionIsInvalid(string requiredPermission, string actualPermission)
    {
        var permission = new Permission(actualPermission);

        Assert.False(permission.GrantsAccessTo(requiredPermission));
    }

    public static IEnumerable<object[]> GetValidPermissions()
    {
        yield return ["Users/Management/Read", "Users/Management/Read"];
        yield return ["Users/Management/Read", "Users/*/Read"];
        yield return ["Users/Management/Roles/Read", "Users/*/Read"];
        yield return ["Users/Management/Roles/Read", "Users/*"];
        yield return ["Users/Management/Roles/Read", "Users/Management/*/Read"];
        yield return ["Users/Management/Roles/Read", "*/Read"];
        yield return ["Users/Management/Roles/Read", "*/Roles/Read"];
    }

    public static IEnumerable<object[]> GetInvalidPermissions()
    {
        yield return ["Users/Management/Read", "Users/Management/Write"];
        yield return ["Users/Management/Read", "Users/*/Write"];
        yield return ["Users/Management/Roles/Read", "Roles/*/Read"];
        yield return ["Users/Management/Roles/Read", "Roles/*"];
        yield return ["Users/Management/Roles/Read", "Users/Management/*/Write"];
        yield return ["Users/Management/Roles/Read", "*/Write"];
        yield return ["Users/Management/Roles/Read", "*/Roles/Write"];
    }
}
