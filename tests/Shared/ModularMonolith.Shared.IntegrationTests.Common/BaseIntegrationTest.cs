using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace ModularMonolith.Shared.IntegrationTests.Common;

[UsesVerify]
public abstract class BaseIntegrationTest<TClass>
{
    protected StringContent GetResource(string name)
    {
        var @namespace = $"{typeof(TClass).Namespace}.Resources.{name}";
        
        using var stream = typeof(TClass).Assembly.GetManifestResourceStream(@namespace);
        var buffer = new byte[stream!.Length];

        _ = stream.Read(buffer);

        return new StringContent(Encoding.UTF8.GetString(buffer), MediaTypeHeaderValue.Parse("application/json"));
    }

    protected async Task VerifyResponse(HttpResponseMessage httpResponseMessage,
        object?[]? parameters = null,
        [CallerMemberName] string method = default!,
        [CallerFilePath] string filePath = default!)
    {
        var type = GetType();
        var attribute = type.GetMethod(method)?.GetCustomAttribute<TestMethodName>();

        string? fileName = null;
        if (attribute is not null)
        {
            fileName = $"{attribute.Name}";
        }
        
        var directory = Path.Join(Path.GetDirectoryName(filePath), "Responses");

        var settings = new VerifySettings();
        
        if (!string.IsNullOrEmpty(fileName))
        {
            settings.UseMethodName(fileName);
        }
        
        settings.UseDirectory(directory);

        if (parameters is not null)
        {
            settings.UseParameters(parameters);
        }

        await VerifyJson(httpResponseMessage.Content.ReadAsStreamAsync(), settings);
    }
}
