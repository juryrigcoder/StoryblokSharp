using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using StoryblokSharp.Models.Configuration;

namespace StoryblokSharp.Services.Cache;

/// <summary>
/// In-memory implementation of IStoryblokCache
/// </summary>
public class MemoryStoryblokCache : IStoryblokCache, IDisposable
{
    private readonly IMemoryCache _cache;
    private readonly Models.Cache.CacheOptions _options;
    private readonly SemaphoreSlim _semaphore;
    private bool _disposed;

    public MemoryStoryblokCache(
        IMemoryCache cache,
        IOptions<StoryblokOptions> options)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _options = options.Value.Cache ?? throw new ArgumentNullException(nameof(options));
        _semaphore = new SemaphoreSlim(1);
    }

    /// <inheritdoc/>
    public async Task<T?> GetAsync<T>(
        string key,
        CancellationToken cancellationToken = default) where T : class
    {
        // Memory cache is synchronous, but we wrap in Task for interface consistency
        await Task.CompletedTask;
        return _cache.Get<T>(key);
    }

    /// <inheritdoc/>
    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default) where T : class
    {
        // Memory cache is synchronous, but we wrap in Task for interface consistency
        await Task.CompletedTask;

        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? _options.DefaultExpiration
        };

        _cache.Set(key, value, options);
    }

    /// <inheritdoc/>
    public async Task RemoveAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        // Memory cache is synchronous, but we wrap in Task for interface consistency
        await Task.CompletedTask;
        _cache.Remove(key);
    }

    /// <inheritdoc/>
    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            if (_cache is MemoryCache memoryCache)
            {
                memoryCache.Compact(1.0);
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _semaphore.Dispose();
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}

/// <summary>
/// No-op implementation of IStoryblokCache that doesn't cache anything
/// </summary>
public class NullStoryblokCache : IStoryblokCache
{
    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
        => Task.FromResult<T?>(null);

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
        => Task.CompletedTask;

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
    public Task ClearAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}