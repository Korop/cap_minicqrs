using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Cap.MiniCqrs.Caching;

public interface IRequestResultCache
{
    Task<T> GetOrAddAsync<T>(string key, Func<CancellationToken, Task<T>> factory, CancellationToken cancellationToken = default);

    Task<T> GetOrAddAsync<T>(string key, Func<CancellationToken, Task<T>> factory, TimeSpan timeToLive, IReadOnlyList<string>? tags = null, CancellationToken cancellationToken = default);

    Task InvalidateAsync(string key, CancellationToken cancellationToken = default);

    Task InvalidateByTagsAsync(IReadOnlyList<string> tags, CancellationToken cancellationToken = default);
}
