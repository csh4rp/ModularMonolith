using MediatR;
using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.Shared.Infrastructure.Queries;

public interface IQueryHandler<in TQuery, TResult> : IRequestHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>, IRequest<TResult>;
