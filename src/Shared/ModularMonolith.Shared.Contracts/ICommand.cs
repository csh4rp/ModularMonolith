using MediatR;

namespace ModularMonolith.Shared.Contracts;

public interface ICommand : IRequest;

public interface ICommand<out T> : IRequest<T>;
