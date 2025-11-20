using System.Threading;
using System.Threading.Tasks;

namespace Cap.MiniCqrs;

public interface IDispatcher
{
    Task<TResponse> Send<TResponse, TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TResponse : notnull
        where TCommand : ICommand<TResponse>;

    Task<TResponse> Query<TResponse, TQuery>(TQuery query, CancellationToken cancellationToken = default)
        where TResponse : notnull
        where TQuery : IQuery<TResponse>;
}
