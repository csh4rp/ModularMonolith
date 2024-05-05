using System.ComponentModel.DataAnnotations;
using ModularMonolith.Shared.DataAccess.Cosmos.AuditLogs.Options;

namespace ModularMonolith.Shared.DataAccess.Cosmos.Options;

public class CosmosOptions
{
    [Required]
    public string DatabaseId { get; set; } = default!;

    public AuditLogOptions AuditLogOptions { get; set; } = new();
}
