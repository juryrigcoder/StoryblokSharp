namespace StoryblokSharp.Services.Cache;

/// <summary>
/// Interface for caching Storyblok API responses
/// </summary>
public interface IStoryblokCache
{
    /// <summary>
    /// Gets a cached value by key
    /// </summary>
    /// <typeparam name="T">The type of the cached value</typeparam>
    /// <param name="key">The cache key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The cached value, or null if not found</returns>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Sets a value in the cache
    /// </summary>
    /// <typeparam name="T">The type of the value to cache</typeparam>
    /// <param name="key">The cache key</param>
    /// <param name="value">The value to cache</param>
    /// <param name="expiration">Optional expiration time</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task that completes when the value is cached</returns>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Removes a value from the cache
    /// </summary>
    /// <param name="key">The cache key to remove</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task that completes when the value is removed</returns>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears all values from the cache
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task that completes when the cache is cleared</returns>
    Task ClearAsync(CancellationToken cancellationToken = default);
}