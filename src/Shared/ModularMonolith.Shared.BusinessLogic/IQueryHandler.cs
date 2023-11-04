using MediatR;
using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.Shared.BusinessLogic;

public interface IQueryHandler<in TQuery, TResult> : IRequestHandler<TQuery, TResult> where TQuery : IQuery<TResult>, IRequest<TResult>
{
}
