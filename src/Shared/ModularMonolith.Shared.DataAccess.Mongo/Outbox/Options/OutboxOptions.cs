using System.ComponentModel.DataAnnotations;

namespace ModularMonolith.Shared.DataAccess.Mongo.Outbox.Options;

public class OutboxOptions
{
    [Required]
    public required string CollectionName { get; set; }
}
