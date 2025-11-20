using System.Threading;
using System.Threading.Tasks;

namespace Cap.MiniCqrs.Caching;

public interface ICachedDispatcher
{
    Task<TResponse> Send<TResponse, TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TResponse : notnull
        where TCommand : ICommand<TResponse>, ICacheKeyProvider;

    Task<TResponse> Query<TResponse, TQuery>(TQuery query, CancellationToken cancellationToken = default)
        where TResponse : notnull
        where TQuery : IQuery<TResponse>, ICacheKeyProvider;
}
