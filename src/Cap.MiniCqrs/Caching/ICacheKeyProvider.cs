namespace Cap.MiniCqrs.Caching;

public interface ICacheKeyProvider
{
    string CacheKey { get; }
}
