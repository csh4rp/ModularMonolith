using ModularMonolith.Shared.Contracts.Errors;

namespace ModularMonolith.Tests.Utils.Assertions;

public static class ErrorAssertionsExtensions
{
    public static ErrorAssertions Should(this Error? result) => new(result);
}
