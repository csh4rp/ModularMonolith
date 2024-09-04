using System.Collections.Concurrent;

namespace ModularMonolith.Shared.Application.Middlewares;

internal abstract class TransactionalTypesDictionary
{
    private static readonly ConcurrentDictionary<Type, bool> TransactionalTypes = new();

    public static bool ShouldUseTransaction(Type type) => TransactionalTypes.GetValueOrDefault(type, false);

    public static void Add(Type type, bool shouldUseTransaction)
    {
        TransactionalTypes.TryAdd(type, shouldUseTransaction);
    }
}
