using System;
using System.Threading;
using System.Threading.Tasks;

namespace Cap.MiniCqrs.Caching;

public sealed class CachedDispatcher : IDispatcher
{
    private readonly IDispatcher _inner;
    private readonly IRequestResultCache _cache;

    public CachedDispatcher(IDispatcher inner, IRequestResultCache cache)
    {
        _inner  = inner ?? throw new ArgumentNullException(nameof(inner));
        _cache  = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public Task<TResponse> Send<TResponse, TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TResponse : notnull
        where TCommand : ICommand<TResponse>
    {
        if (command is not ICacheKeyProvider cacheKeyProvider)
        {
            return _inner.Send<TResponse, TCommand>(command, cancellationToken);
        }

        return _cache.GetOrAddAsync(cacheKeyProvider.CacheKey,
            static (token, state) => state.dispatcher.Send<TResponse, TCommand>(state.command, token),
            (dispatcher: _inner, command),
            cancellationToken);
    }

    public Task<TResponse> Query<TResponse, TQuery>(TQuery query, CancellationToken cancellationToken = default)
        where TResponse : notnull
        where TQuery : IQuery<TResponse>
    {
        if (query is not ICacheKeyProvider cacheKeyProvider)
        {
            return _inner.Query<TResponse, TQuery>(query, cancellationToken);
        }

        return _cache.GetOrAddAsync(cacheKeyProvider.CacheKey,
            static (token, state) => state.dispatcher.Query<TResponse, TQuery>(state.query, token),
            (dispatcher: _inner, query),
            cancellationToken);
    }
}

internal static class RequestResultCacheExtensions
{
    public static Task<T> GetOrAddAsync<T, TState>(this IRequestResultCache cache, string key,
        Func<CancellationToken, TState, Task<T>> factory, TState state, CancellationToken cancellationToken)
        => cache.GetOrAddAsync(key, ct => factory(ct, state), cancellationToken);
}
