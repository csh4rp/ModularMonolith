using System.ComponentModel.DataAnnotations;

namespace ModularMonolith.Shared.DataAccess.Mongo.Outbox.Options;

public class OutboxOptions
{
    [Required]
    public required string CollectionName { get; set; }

    [Required]
    public required TimeSpan ChangeDataCaptureDiff { get; set; } = TimeSpan.FromMinutes(15);

    [Required]
    public required int ChangeDataCaptureBatchSize { get; set; } = 100;
}
