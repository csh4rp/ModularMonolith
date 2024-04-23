using System.Collections;
using System.Collections.Immutable;

namespace ModularMonolith.Shared.DataAccess.Models;

public record DataPage<T>(ImmutableArray<T> Items, long TotalCount) : IReadOnlyList<T>
{
    public int Count => Items.Length;

    public T this[int index] => Items[index];

    public IEnumerator<T> GetEnumerator() => Items.AsEnumerable().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
