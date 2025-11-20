using System;
using System.Threading;
using System.Threading.Tasks;

namespace Cap.MiniCqrs.Caching;

public sealed class StrictCachedDispatcher : ICachedDispatcher
{
    private readonly IDispatcher _dispatcher;
    private readonly IRequestResultCache _cache;

    public StrictCachedDispatcher(IDispatcher dispatcher, IRequestResultCache cache)
    {
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public Task<TResponse> Send<TResponse, TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TResponse : notnull
        where TCommand : ICommand<TResponse>, ICacheKeyProvider
    {
        return _cache.GetOrAddAsync(command.CacheKey,
            ct => _dispatcher.Send<TResponse, TCommand>(command, ct),
            cancellationToken);
    }

    public Task<TResponse> Query<TResponse, TQuery>(TQuery query, CancellationToken cancellationToken = default)
        where TResponse : notnull
        where TQuery : IQuery<TResponse>, ICacheKeyProvider
    {
        return _cache.GetOrAddAsync(query.CacheKey,
            ct => _dispatcher.Query<TResponse, TQuery>(query, ct),
            cancellationToken);
    }
}
