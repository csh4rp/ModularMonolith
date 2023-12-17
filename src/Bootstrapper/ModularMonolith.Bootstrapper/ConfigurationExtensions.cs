using System.Reflection;
using ModularMonolith.Shared.Api;

namespace ModularMonolith.Bootstrapper;

public static class ConfigurationExtensions
{
    public static IEnumerable<AppModule> GetEnabledModules(this IConfiguration configuration)
    {
        var assemblies = Assembly.GetExecutingAssembly().GetReferencedAssemblies();
        var modulesSection = configuration.GetRequiredSection("Modules");

        foreach (var moduleSection in modulesSection.GetChildren())
        {
            if (!modulesSection.GetValue<bool>("Enabled"))
            {
                continue;
            }

            var name = moduleSection.Key;

            var appModuleType = assemblies
                .Where(a => a.Name!.Contains(name))
                .Select(Assembly.Load)
                .SelectMany(a => a.GetExportedTypes())
                .First(t => t.IsAssignableTo(typeof(AppModule)));

            yield return (AppModule) Activator.CreateInstance(appModuleType)!;
        }
    }
}
