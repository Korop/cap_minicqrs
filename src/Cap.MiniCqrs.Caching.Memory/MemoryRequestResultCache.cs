using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cap.MiniCqrs.Caching;
using Microsoft.Extensions.Caching.Hybrid;

namespace Cap.MiniCqrs.Caching.Memory;

public sealed class MemoryRequestResultCache : IRequestResultCache
{
    private readonly HybridCache _cache;
    private readonly TimeSpan _defaultTtl;

    public MemoryRequestResultCache(HybridCache cache, TimeSpan? ttl = null)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _defaultTtl = ttl ?? TimeSpan.FromMinutes(5);
    }

    public Task<T> GetOrAddAsync<T>(string key, Func<CancellationToken, Task<T>> factory, CancellationToken cancellationToken = default)
        => GetOrAddAsync(key, factory, _defaultTtl, tags: null, cancellationToken);

    public async Task<T> GetOrAddAsync<T>(string key, Func<CancellationToken, Task<T>> factory, TimeSpan timeToLive, IReadOnlyList<string>? tags = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Cache key must not be null or whitespace.", nameof(key));
        }

        if (factory is null)
        {
            throw new ArgumentNullException(nameof(factory));
        }

        if (timeToLive <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(timeToLive), "TTL must be positive.");
        }

        var options = new HybridCacheEntryOptions
        {
            Expiration = timeToLive,
            LocalCacheExpiration = timeToLive
        };

        return await _cache
            .GetOrCreateAsync(
                key,
                async ct => await factory(ct).ConfigureAwait(false),
                options,
                tags,
                cancellationToken)
            .ConfigureAwait(false);
    }

    public Task InvalidateAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Cache key must not be null or whitespace.", nameof(key));
        }

        return _cache.RemoveAsync(key, cancellationToken).AsTask();
    }

    public async Task InvalidateByTagsAsync(IReadOnlyList<string> tags, CancellationToken cancellationToken = default)
    {
        if (tags is null || tags.Count == 0)
        {
            throw new ArgumentException("Tags must contain at least one entry.", nameof(tags));
        }

        foreach (var tag in tags)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                throw new ArgumentException("Tags cannot contain null/empty values.", nameof(tags));
            }

            await _cache.RemoveByTagAsync(tag, cancellationToken).ConfigureAwait(false);
        }
    }
}
