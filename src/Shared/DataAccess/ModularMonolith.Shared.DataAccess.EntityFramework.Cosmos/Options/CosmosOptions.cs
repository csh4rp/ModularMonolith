using System.ComponentModel.DataAnnotations;
using Azure.Core;

namespace ModularMonolith.Shared.DataAccess.EntityFramework.Cosmos.Options;

public class CosmosOptions
{
    [Required]
    public string AccountEndpoint { get; set; } = default!;

    [Required]
    public string DatabaseName { get; set; } = default!;

    public string? AccountKey { get; set; }

    public TokenCredential? TokenCredential { get; set; }

    public bool UseAuditLogInterceptor { get; set; } = true;

    public bool UseEventLogInterceptor { get; set; } = true;

    public bool UseSnakeCaseNamingConvention { get; set; }

}
