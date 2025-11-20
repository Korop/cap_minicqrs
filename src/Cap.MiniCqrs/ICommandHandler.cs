using System.Threading;
using System.Threading.Tasks;

namespace Cap.MiniCqrs;

public interface ICommandHandler
{
    Task<object> Handle(object command, CancellationToken cancellationToken = default);
}

public interface ICommandHandler<TCommand, TResponse> : ICommandHandler
    where TCommand : ICommand<TResponse>
{
    Task<TResponse> Handle(TCommand command, CancellationToken cancellationToken = default);
}
