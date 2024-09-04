using MassTransit;
using ModularMonolith.Shared.Identity;

namespace ModularMonolith.Shared.Messaging.MassTransit.Filters;

public class IdentityFilter<T> : IFilter<ConsumeContext<T>> where T : class
{
    private readonly IIdentityContextSetter _identityContextSetter;

    public IdentityFilter(IIdentityContextSetter identityContextSetter)
    {
        _identityContextSetter = identityContextSetter;
    }

    public Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        if (context.TryGetHeader<string>("subject", out var subject))
        {
            // _identityContextSetter.Set(new IdentityContext(subject, [Permission.Parse("*")]));
        }

        return next.Send(context);
    }

    public void Probe(ProbeContext context)
    {
    }
}
