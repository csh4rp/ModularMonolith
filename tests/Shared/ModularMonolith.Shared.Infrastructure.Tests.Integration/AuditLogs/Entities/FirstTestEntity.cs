namespace ModularMonolith.Shared.Infrastructure.Tests.Integration.AuditLogs.Entities;

public class FirstTestEntity
{
    public Guid Id { get; init; }
    
    public string? Name { get; set; }
    
    public string? Sensitive { get; set; }

    public OwnedEntity? OwnedEntity { get; set; }
    
    public IList<SecondTestEntity> SecondEntities { get; set; } = new List<SecondTestEntity>();
}
