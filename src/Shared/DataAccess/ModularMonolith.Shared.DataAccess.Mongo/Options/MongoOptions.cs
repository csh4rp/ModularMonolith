using System.ComponentModel.DataAnnotations;
using ModularMonolith.Shared.DataAccess.Mongo.AuditLogs.Options;

namespace ModularMonolith.Shared.DataAccess.Mongo.Options;

public class MongoOptions
{
    [Required]
    public string ConnectionStringName { get; set; } = default!;

    [Required]
    public string DatabaseName { get; set; } = default!;

    public AuditLogOptions AuditLogOptions { get; set; } = new();
}
