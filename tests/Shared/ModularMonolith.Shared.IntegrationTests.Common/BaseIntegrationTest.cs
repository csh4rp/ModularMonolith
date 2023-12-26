using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace ModularMonolith.Shared.IntegrationTests.Common;

[UsesVerify]
public abstract class BaseIntegrationTest<TClass, TProgram> where TProgram : class
{
    protected HttpClient HttpClient { get; }

    protected BaseIntegrationTest(IDatabaseFixture databasePostgresFixture)
    {
        var factory = new WebApplicationFactory<TProgram>()
            .WithWebHostBuilder(b =>
            {
                b.ConfigureAppConfiguration((c, s) =>
                {
                    s.AddInMemoryCollection(new []
                    {
                        new KeyValuePair<string, string?>("ConnectionStrings:Database", databasePostgresFixture.ConnectionString)
                    });
                });
            });

        HttpClient = factory.CreateClient();
    }

    protected StringContent GetResource(string name)
    {
        var @namespace = $"{typeof(TClass).Namespace}.Resources.{name}";
        
        using var stream = typeof(TClass).Assembly.GetManifestResourceStream(@namespace);
        var buffer = new byte[stream!.Length];

        _ = stream.Read(buffer);

        return new StringContent(Encoding.UTF8.GetString(buffer), MediaTypeHeaderValue.Parse("application/json"));
    }

    protected async Task VerifyResponse(HttpResponseMessage httpResponseMessage,
        [CallerMemberName] string method = default!,
        [CallerFilePath] string filePath = default!)
    {
        var type = GetType();
        var attribute = type.GetMethod(method)?.GetCustomAttribute<HasFileName>();

        string? fileName = null;
        if (attribute is not null)
        {
            fileName = $"{type.Name}.{attribute.FileName}";
        }
        
        var directory = Path.Join(Path.GetDirectoryName(filePath), "Responses");

        var task =  VerifyJson(httpResponseMessage.Content.ReadAsStreamAsync())
            .UseDirectory(directory);

        if (!string.IsNullOrEmpty(fileName))
        {
            task = task.UseFileName(fileName);
        }

        await task;
    }
}
