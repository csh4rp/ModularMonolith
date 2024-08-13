namespace ModularMonolith.Shared.DataAccess.EntityFramework.SqlServer.IntegrationTests.AuditLogs.Entities;

public class SecondTestEntity
{
    public required Guid Id { get; set; }

    public required string Name { get; set; }

    public IList<FirstTestEntity> FirstTestEntities { get; set; } = new List<FirstTestEntity>();
}
