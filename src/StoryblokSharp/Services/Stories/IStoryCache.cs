using StoryblokSharp.Models.Stories;

namespace StoryblokSharp.Services.Stories;

/// <summary>
/// Service for caching story responses
/// </summary>
public interface IStoryCache
{
    /// <summary>
    /// Gets a cached story response
    /// </summary>
    Task<StoryResponse<T>?> GetAsync<T>(
        string key,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Sets a story response in cache
    /// </summary>
    Task SetAsync<T>(
        string key,
        StoryResponse<T> response,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Gets the current cache version
    /// </summary>
    long GetCacheVersion();

    /// <summary>
    /// Sets the cache version
    /// </summary>
    void SetCacheVersion(long version);

    /// <summary>
    /// Clears the cache
    /// </summary>
    Task ClearAsync(CancellationToken cancellationToken = default);
}