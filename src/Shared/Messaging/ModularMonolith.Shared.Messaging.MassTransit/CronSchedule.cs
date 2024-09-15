using MassTransit.Scheduling;

namespace ModularMonolith.Shared.Messaging.MassTransit;

public class CronSchedule : DefaultRecurringSchedule
{
    public CronSchedule(string cron)
    {
        CronExpression = cron;
    }
}
