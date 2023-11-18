using MassTransit;
using ModularMonolith.Shared.BusinessLogic.Abstract;

namespace ModularMonolith.Shared.Infrastructure.Events;

public class TransactionEnlistingEventFilter<T>(ITransactionalScope transactionalScope) : IFilter<ConsumeContext<T>>
    where T : class
{
    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        await next.Send(context);

        await transactionalScope.CompleteAsync(context.CancellationToken);
    }

    public void Probe(ProbeContext context)
    {
    }
}
