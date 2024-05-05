using System.ComponentModel.DataAnnotations;

namespace ModularMonolith.Shared.DataAccess.Cosmos.AuditLogs.Options;

public class AuditLogOptions
{
    [Required]
    public string ContainerName { get; set; } = "AuditLogs";
}
