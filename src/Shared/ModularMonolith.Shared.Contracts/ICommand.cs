using MediatR;

namespace ModularMonolith.Shared.Contracts;

public interface ICommand : IRequest<Result>;

public interface ICommand<T> : IRequest<Result<T>>;
