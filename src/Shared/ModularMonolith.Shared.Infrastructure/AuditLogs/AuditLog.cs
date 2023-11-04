namespace ModularMonolith.Shared.Infrastructure.AuditLogs;

public class AuditLog
{
    public long Id { get; init; }
    
    public required DateTimeOffset CreatedAt { get; init; }
    
    public required ChangeType ChangeType { get; init; }
    
    public required string EntityType { get; init; }
    
    public required string EntityKeys { get; init; }
    
    public required string Changes { get; init; }
}
