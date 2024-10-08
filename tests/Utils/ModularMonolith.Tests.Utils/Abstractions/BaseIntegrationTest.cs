﻿using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using ModularMonolith.Shared.Events;

[assembly: ExcludeFromCodeCoverage]

namespace ModularMonolith.Tests.Utils.Abstractions;

public abstract class BaseIntegrationTest<TClass> : IAsyncLifetime
{
    protected StringContent GetResource(string name)
    {
        var @namespace = $"{typeof(TClass).Namespace}.Requests.{name}";

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
        var attribute = type.GetMethod(method)?.GetCustomAttribute<TestFileNameAttribute>();

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

    protected async Task VerifyMessage<T>(T message,
        object?[]? parameters = null,
        [CallerMemberName] string method = default!,
        [CallerFilePath] string filePath = default!)
    {
        var messageType = typeof(T);
        var fileName = messageType.GetCustomAttribute<EventAttribute>()?.Name
                       ?? messageType.Name;

        var directory = Path.Join(Path.GetDirectoryName(filePath), "Messages");
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

        await Verify(message, settings);
    }

    public virtual Task InitializeAsync() => Task.CompletedTask;

    public virtual Task DisposeAsync() => Task.CompletedTask;
}
