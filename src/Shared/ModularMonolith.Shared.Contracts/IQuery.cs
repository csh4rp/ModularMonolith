using MediatR;

namespace ModularMonolith.Shared.Contracts;

public interface IQuery<out T> : IRequest<T>;
