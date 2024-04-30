using System.ComponentModel.DataAnnotations;

namespace ModularMonolith.Shared.DataAccess.Mongo.EventLogs.Options;

public class EventOptions
{
    [Required]
    public required string CollectionName { get; set; }
}
