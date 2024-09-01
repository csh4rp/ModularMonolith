namespace ModularMonolith.Shared.Identity.UnitTests;

public class PermissionTests
{
    [Theory]
    [MemberData(nameof(GetPermissions))]
    public void Should(string requiredPermission, string actualPermission)
    {
        var permission = new Permission(actualPermission);

        Assert.True(permission.GrantsAccessTo(requiredPermission));
    }

    public static IEnumerable<object[]> GetPermissions()
    {
        yield return ["Users/Management/Read", "Users/Management/Read"];
        yield return ["Users/Management/Read", "Users/*/Read"];
        yield return ["Users/Management/Roles/Read", "Users/*/Read"];
        yield return ["Users/Management/Roles/Read", "Users/*"];
        yield return ["Users/Management/Roles/Read", "Users/Management/*/Read"];
        yield return ["Users/Management/Roles/Read", "*/Read"];
    }
}
