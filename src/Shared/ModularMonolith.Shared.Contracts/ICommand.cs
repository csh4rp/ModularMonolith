using MediatR;

namespace ModularMonolith.Shared.Contracts;

public interface ICommand : IRequest;

public interface ICommand<T> : IRequest<T>;
