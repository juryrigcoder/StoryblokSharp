using StoryblokSharp.Models.Stories;

namespace StoryblokSharp.Client;

/// <summary>
/// Main interface for interacting with the Storyblok API
/// </summary>
public interface IStoryblokClient : IAsyncDisposable
{
    /// <summary>
    /// Gets a single story by slug
    /// </summary>
    Task<StoryResponse<T>> GetStoryAsync<T>(
        string slug, 
        StoryQueryParameters? parameters = null, 
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Gets multiple stories based on query parameters
    /// </summary>
    Task<StoriesResponse<T>> GetStoriesAsync<T>(
        StoryQueryParameters parameters, 
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Gets all stories matching the query parameters
    /// </summary>
    Task<IEnumerable<Story<T>>> GetAllAsync<T>(
        string endpoint,
        StoryQueryParameters parameters,
        string? entity = null,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Gets the cache version for the current token
    /// </summary>
    int GetCacheVersion();

    /// <summary>
    /// Sets the cache version for the current token
    /// </summary>
    void SetCacheVersion(int version);

    /// <summary>
    /// Clears the cache version for the current token
    /// </summary>
    void ClearCacheVersion();

    /// <summary>
    /// Clears all cached data
    /// </summary>
    Task ClearCacheAsync(CancellationToken cancellationToken = default);
}