using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace ModularMonolith.Shared.Infrastructure;

public static class ConfigurationExtensions
{
    public static IEnumerable<IAppModule> GetEnabledModules(this IConfiguration configuration)
    {
        var executingAssembly = Assembly.GetExecutingAssembly();
        var location = executingAssembly.Location;
        var currentDirectory = Path.GetDirectoryName(location);

        var files = Directory.GetFiles(currentDirectory!)
            .ToList();

        var modulesSection = configuration.GetRequiredSection("Modules");

        foreach (var moduleSection in modulesSection.GetChildren())
        {
            if (!string.Equals(moduleSection.GetSection("Enabled").Value, bool.TrueString,
                    StringComparison.InvariantCultureIgnoreCase))
            {
                continue;
            }

            var name = moduleSection.Key;

            var appModuleType = files
                .Where(file => file.EndsWith($".{name}.Infrastructure.dll"))
                .Select(Assembly.LoadFile)
                .SelectMany(assembly => assembly.GetExportedTypes())
                .First(t => t.IsAssignableTo(typeof(IAppModule)));

            yield return (IAppModule)Activator.CreateInstance(appModuleType)!;
        }
    }
}
