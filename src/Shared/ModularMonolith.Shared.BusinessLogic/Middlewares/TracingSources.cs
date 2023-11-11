using System.Diagnostics;

namespace ModularMonolith.Shared.BusinessLogic.Middlewares;

internal static class TracingSources
{
    public static readonly ActivitySource Default = new("Default");
}
