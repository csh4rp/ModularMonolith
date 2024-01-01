using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.Shared.TestUtils.Assertions;

public static class ResultAssertionsExtensions
{
    public static ResultAssertions Should(this Result result) => new(result);

    public static ResultAssertions<T> Should<T>(this Result<T> result) => new(result);
}
