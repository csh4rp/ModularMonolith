using System.ComponentModel.DataAnnotations;

namespace ModularMonolith.Shared.DataAccess.EntityFramework.Options;

public class EntityFrameworkDataAccessOptions
{
    public bool UseAuditLogInterceptor { get; set; } = true;

    public bool UseEventLogInterceptor { get; set; } = true;

    public bool UseSnakeCaseNamingConvention { get; set; }

    [Required]
    public string ConnectionStringName { get; set; } = default!;
}
