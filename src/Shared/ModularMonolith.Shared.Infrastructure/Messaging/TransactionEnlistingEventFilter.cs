using MassTransit;
using ModularMonolith.Shared.BusinessLogic.Abstract;

namespace ModularMonolith.Shared.Infrastructure.Messaging;

public class TransactionEnlistingEventFilter<T>(ITransactionalScopeFactory transactionalScopeFactory) 
    : IFilter<ConsumeContext<T>> where T : class
{
    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        await using var scope = transactionalScopeFactory.Create();
        
        await next.Send(context);

        await scope.CompleteAsync(context.CancellationToken);
    }

    public void Probe(ProbeContext context)
    {
    }
}
