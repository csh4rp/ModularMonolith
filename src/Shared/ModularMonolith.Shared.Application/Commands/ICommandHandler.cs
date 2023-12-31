using MediatR;
using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.Shared.Application.Commands;

public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Result> where TCommand : ICommand;

public interface ICommandHandler<in TCommand, TResult> : IRequestHandler<TCommand, Result<TResult>>
    where TCommand : ICommand<TResult>;
