namespace ModularMonolith.Shared.Messaging.MassTransit;

public abstract class MessagingConstants
{
    public const string ScheduleQueueName = "queue:quartz";

    public static readonly Uri ScheduleQueueUri = new(ScheduleQueueName);
}
