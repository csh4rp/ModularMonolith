using System.Collections;
using System.Collections.Frozen;

namespace ModularMonolith.Shared.DataAccess.AudiLog;

public sealed class EntityKey : IEnumerable<EntityField>
{
    private readonly FrozenDictionary<string, EntityField> _fields;

    public EntityKey(IEnumerable<EntityField> fields) => _fields = fields.ToFrozenDictionary(k => k.Name);

    public IReadOnlyList<string> FieldNames => _fields.Keys;

    public object? this[string fieldName] => _fields[fieldName].Value;

    public bool TryGetFieldValue(string fieldName, out object? value)
    {
        if (!_fields.TryGetValue(fieldName, out var entityField))
        {
            value = null;
            return false;
        }

        value = entityField.Value;
        return true;
    }

    public IEnumerator<EntityField> GetEnumerator() => _fields.Values.AsEnumerable().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
