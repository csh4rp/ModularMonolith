using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ModularMonolith.Shared.Infrastructure.Events.Options;

public class EventOptions
{
    [Range(typeof(TimeSpan), "00:00:01", "01:00:00")]
    public TimeSpan PollInterval { get; set; }

    [Range(typeof(TimeSpan), "00:00:01", "01:00:00")]
    public TimeSpan MaxLockTime { get; set; }

    [Range(typeof(TimeSpan), "00:00:01", "01:00:00")]
    public TimeSpan TimeBetweenAttempts { get; set; }

    [Required]
    public List<Assembly> Assemblies { get; set; } = new();

    [Range(1, int.MaxValue)]
    public int MaxParallelWorkers { get; set; }

    [Range(1, int.MaxValue)]
    public int MaxRetryAttempts { get; set; }

    [Range(1, int.MaxValue)]
    public int MaxPollBatchSize { get; set; }
    
    [Range(1, int.MaxValue)]
    public int MaxEventChannelSize { get; set; }

    [DefaultValue(true)]
    public bool RunBackgroundWorkers { get; set; }
}
