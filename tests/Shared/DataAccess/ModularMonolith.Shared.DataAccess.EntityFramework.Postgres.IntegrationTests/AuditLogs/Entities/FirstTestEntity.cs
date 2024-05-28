namespace ModularMonolith.Shared.DataAccess.EntityFramework.Postgres.IntegrationTests.AuditLogs.Entities;

public class FirstTestEntity
{
    public required long Id { get; set; }
    
    public required DateTimeOffset Timestamp { get; set; }
    
    public required string Name { get; set; }
    
    public required OwnedEntity OwnedEntity { get; set; }
    
    public IList<SecondTestEntity> SecondTestEntities { get; set; } = new List<SecondTestEntity>();
}
