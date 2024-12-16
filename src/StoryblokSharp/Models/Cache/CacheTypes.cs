namespace StoryblokSharp.Models.Cache;

/// <summary>
/// Cache configuration options
/// </summary>
public record CacheOptions
{
    /// <summary>
    /// Type of caching to use
    /// </summary>
    public CacheType Type { get; init; } = CacheType.Memory;

    /// <summary>
    /// When to clear the cache
    /// </summary>
    public CacheClearMode Clear { get; init; } = CacheClearMode.Manual;

    /// <summary>
    /// Default expiration time for cache entries
    /// </summary>
    public TimeSpan? DefaultExpiration { get; init; }

    /// <summary>
    /// Custom cache implementation
    /// </summary>
    public ICacheProvider? Custom { get; init; }
}

/// <summary>
/// Type of cache storage
/// </summary>
public enum CacheType
{
    /// <summary>
    /// No caching
    /// </summary>
    None,

    /// <summary>
    /// In-memory caching
    /// </summary>
    Memory,

    /// <summary>
    /// Custom cache implementation
    /// </summary>
    Custom
}

/// <summary>
/// Cache clearing mode
/// </summary>
public enum CacheClearMode
{
    /// <summary>
    /// Cache is cleared manually
    /// </summary>
    Manual,

    /// <summary>
    /// Cache is cleared automatically when draft version is requested
    /// </summary>
    Auto
}

/// <summary>
/// Builder for cache options
/// </summary>
public class CacheOptionsBuilder
{
    private CacheType _type = CacheType.Memory;
    private CacheClearMode _clear = CacheClearMode.Manual;
    private TimeSpan? _defaultExpiration;
    private ICacheProvider? _custom;

    /// <summary>
    /// Sets the cache type
    /// </summary>
    public CacheOptionsBuilder WithType(CacheType type)
    {
        _type = type;
        return this;
    }

    /// <summary>
    /// Sets the cache clear mode
    /// </summary>
    public CacheOptionsBuilder WithClearMode(CacheClearMode mode)
    {
        _clear = mode;
        return this;
    }

    /// <summary>
    /// Sets the default expiration time
    /// </summary>
    public CacheOptionsBuilder WithDefaultExpiration(TimeSpan expiration)
    {
        _defaultExpiration = expiration;
        return this;
    }

    /// <summary>
    /// Sets the custom cache provider
    /// </summary>
    public CacheOptionsBuilder WithCustomProvider(ICacheProvider provider)
    {
        _custom = provider;
        _type = CacheType.Custom;
        return this;
    }

    /// <summary>
    /// Builds the cache options
    /// </summary>
    internal CacheOptions Build()
    {
        return new CacheOptions
        {
            Type = _type,
            Clear = _clear,
            DefaultExpiration = _defaultExpiration,
            Custom = _custom
        };
    }
}
/// <summary>
/// Interface for custom cache implementations
/// </summary>
public interface ICacheProvider
{
    /// <summary>
    /// Gets a value from the cache
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Sets a value in the cache
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Gets all cached values
    /// </summary>
    Task<IDictionary<string, object>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Flushes all values from the cache
    /// </summary>
    Task FlushAsync(CancellationToken cancellationToken = default);
}