using System.ComponentModel.DataAnnotations;

namespace ModularMonolith.Shared.DataAccess.Options;

public sealed class DatabaseOptions
{
    [Required]
    public string ConnectionString { get; set; } = default!;
}
