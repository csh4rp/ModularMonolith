using System.ComponentModel.DataAnnotations;

namespace ModularMonolith.Identity.Core.Options;

public sealed class AuthOptions
{
    [Required]
    public required string Key { get; set; }

    [Required]
    public required string Audience { get; set; }

    [Required]
    public required string Issuer { get; set; }

    [Required]
    public int ExpirationTimeInMinutes { get; set; }
}
