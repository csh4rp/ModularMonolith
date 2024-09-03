namespace ModularMonolith.Shared.Identity.UnitTests;

public class PermissionTests
{
    [Theory]
    [MemberData(nameof(GetValidPermissions))]
    public void ShouldParse_WhenValueIsValid(string value)
    {
        var permission = Permission.Parse(value);

        Assert.NotNull(permission);
    }

    [Theory]
    [MemberData(nameof(GetInvalidPermissions))]
    public void ShouldThrowException_WhenValueIsInvalid(string value)
    {
        Assert.Throws<ArgumentException>(() => Permission.Parse(value));
    }

    [Theory]
    [MemberData(nameof(GetMatchingPermissions))]
    public void ShouldGrantAccess_WhenActualPermissionIsValid(string requiredPermission, string actualPermission)
    {
        var permission = Permission.Parse(actualPermission);

        Assert.True(permission.GrantsAccessTo(requiredPermission));
    }

    [Theory]
    [MemberData(nameof(GetNotMatchingPermissions))]
    public void ShouldNotGrantAccess_WhenActualPermissionIsInvalid(string requiredPermission, string actualPermission)
    {
        var permission = Permission.Parse(actualPermission);

        Assert.False(permission.GrantsAccessTo(requiredPermission));
    }

    public static IEnumerable<object[]> GetValidPermissions()
    {
        yield return ["Users/Management/Read"];
        yield return ["Users/Management/*"];
        yield return ["Users/*/Read"];
        yield return ["Users/*"];
        yield return ["*"];
        yield return ["*/Management"];
        yield return ["*/Management/Read"];
    }

    public static IEnumerable<object[]> GetInvalidPermissions()
    {
        yield return ["Users/*Read"];
        yield return ["/*"];
        yield return ["Users.Read"];
    }

    public static IEnumerable<object[]> GetMatchingPermissions()
    {
        yield return ["Users/Management/Read", "Users/Management/Read"];
        yield return ["Users/Management/Read", "Users/*/Read"];
        yield return ["Users/Management/Roles/Read", "Users/*/Read"];
        yield return ["Users/Management/Roles/Read", "Users/*"];
        yield return ["Users/Management/Roles/Read", "Users/Management/*/Read"];
        yield return ["Users/Management/Roles/Read", "*/Read"];
        yield return ["Users/Management/Roles/Read", "*/Roles/Read"];
    }

    public static IEnumerable<object[]> GetNotMatchingPermissions()
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
