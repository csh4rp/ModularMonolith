using System.Diagnostics;
using MassTransit;

namespace ModularMonolith.Shared.Infrastructure.Messaging.Filters;

internal sealed class TracingEventFilter<T> : IFilter<ConsumeContext<T>> where T : class
{
    private readonly ActivitySource _activitySource = new("");
    
    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        var traceId = context.Headers.Get<string>("ActivityId");

        var operationName = typeof(T).Name;

        using var activity = _activitySource.StartActivity(operationName, ActivityKind.Internal, traceId);
        
        await next.Send(context);
    }

    public void Probe(ProbeContext context)
    {
        
    }
}
