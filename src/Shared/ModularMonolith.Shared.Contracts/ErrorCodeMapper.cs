using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;

namespace ModularMonolith.Shared.Contracts;

public static class ErrorCodeMapper
{
    private static readonly FrozenDictionary<string, string> Mappings =
        new Dictionary<string, string> { { "NotEmptyValidator", ErrorCodes.Required } }.ToFrozenDictionary();

    public static bool TryMap(string code, [NotNullWhen(true)] out string? mappedCode) =>
        Mappings.TryGetValue(code, out mappedCode);
}
