using System.Dynamic;
using ModularMonolith.Shared.DataAccess.AudiLogs;

namespace ModularMonolith.Shared.DataAccess.Cosmos.Common;

public sealed class CosmosDocument : DynamicObject
{
    private readonly object _object;
    private readonly Dictionary<string, object?> _properties;

    public CosmosDocument(object obj)
    {
        var type = obj.GetType();

        _object = obj;
        _properties = type.GetProperties()
            .ToDictionary(k => k.Name, v => v.GetValue(obj));

        _properties.Add("__type", type.FullName);
    }

    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
        if (_properties.TryGetValue(binder.Name, out var property))
        {
            result = property;
            return true;
        }

        result = null;
        return false;
    }

    public override bool TrySetMember(SetMemberBinder binder, object? value)
    {
        _properties[binder.Name] = value;
        return true;
    }

    public override IEnumerable<string> GetDynamicMemberNames() => _properties.Keys;

    public object GetWrappedObject() => _object;

    public void SetAuditMetaData(AuditMetaData auditMetaData)
    {
        _properties.Add("__audit", auditMetaData);
    }
}
