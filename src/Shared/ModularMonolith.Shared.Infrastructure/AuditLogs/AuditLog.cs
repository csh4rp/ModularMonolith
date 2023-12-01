namespace ModularMonolith.Shared.Infrastructure.AuditLogs;

public class AuditLog
{
    public Guid Id { get; init; }
    
    public required DateTimeOffset CreatedAt { get; init; }
    
    public required string EntityType { get; init; }
    
    public required string EntityKeys { get; init; }
    
    public required ChangeType ChangeType { get; init; }
    
    public required string Changes { get; init; }
    
    public Guid? UserId { get; init; }
    
    public required string OperationName { get; init; }
    
    public required string ActivityId { get; init; }
}
