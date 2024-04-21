namespace ModularMonolith.Shared.AuditTrail.Mongo.Mapping;

public interface IEntityClassMap
{
    string CollectionName { get; }

    bool UsesAuditTrail { get; }
}
