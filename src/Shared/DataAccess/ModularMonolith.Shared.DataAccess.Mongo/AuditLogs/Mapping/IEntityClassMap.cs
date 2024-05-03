namespace ModularMonolith.Shared.DataAccess.Mongo.AuditLogs.Mapping;

public interface IEntityClassMap
{
    string CollectionName { get; }

    bool UsesAuditTrail { get; }
}
