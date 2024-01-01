using ModularMonolith.Shared.Contracts.Errors;

namespace ModularMonolith.Shared.TestUtils.Assertions;

public static class ErrorAssertionsExtensions
{
    public static ErrorAssertions Should(this Error? result) => new(result);
}
