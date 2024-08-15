namespace ModularMonolith.Shared.DataAccess.EntityFramework.SqlServer.IntegrationTests.AuditLogs.Entities;

public class FirstTestEntity
{
    public required Guid Id { get; set; }

    public required DateTimeOffset Timestamp { get; set; }

    public required string Name { get; set; }

    public required FirstOwnedEntity FirstOwnedEntity { get; set; }

    public required SecondOwnedEntity SecondOwnedEntity { get; set; }

    public IList<SecondTestEntity> SecondTestEntities { get; set; } = new List<SecondTestEntity>();
}
