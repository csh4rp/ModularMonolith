using System.Reflection;

namespace ModularMonolith.Shared.RestApi;

public static class ConfigurationExtensions
{
    public static IEnumerable<IWebAppModule> GetEnabledWebModules(this IConfiguration configuration)
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
                .Where(file => file.EndsWith($".{name}.RestApi.dll"))
                .Select(Assembly.LoadFile)
                .SelectMany(assembly => assembly.GetExportedTypes())
                .FirstOrDefault(t => t.IsAssignableTo(typeof(IWebAppModule)));

            if (appModuleType is null)
            {
                continue;
            }

            yield return (IWebAppModule)Activator.CreateInstance(appModuleType)!;
        }
    }
}
