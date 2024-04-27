using System.Collections;
using System.Collections.Frozen;

namespace ModularMonolith.Shared.DataAccess.AudiLog;

public sealed record EntityChanges : IEnumerable<EntityFieldChange>
{
    private readonly FrozenDictionary<string, EntityFieldChange> _changes;

    public EntityChanges(IEnumerable<EntityFieldChange> changes) => _changes = changes.ToFrozenDictionary(k => k.Name);

    public IReadOnlyList<string> FieldNames => _changes.Keys;

    public (object? OriginalValue, object? CurrentValue) this[string fieldName]
    {
        get
        {
            var change = _changes[fieldName];
            return (change.OriginalValue, change.CurrentValue);
        }
    }

    public bool TryGetFieldValues(string fieldName, out (object? OriginalValue, object? CurrentValue) value)
    {
        if (!_changes.TryGetValue(fieldName, out var change))
        {
            value = default;
            return false;
        }

        value = (change.OriginalValue, change.CurrentValue);
        return true;
    }

    public IEnumerator<EntityFieldChange> GetEnumerator() => _changes.Values.AsEnumerable().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
