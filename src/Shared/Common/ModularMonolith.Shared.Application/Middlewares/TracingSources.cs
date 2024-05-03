using System.Diagnostics;

namespace ModularMonolith.Shared.Application.Middlewares;

internal static class TracingSources
{
    public static readonly ActivitySource Default = new("Default");
}
