using StoryblokSharp.Models.Stories;

namespace StoryblokSharp.Services.Stories;

/// <summary>
/// Main service for interacting with Storyblok stories
/// </summary>
public interface IStoryService
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
    /// Gets all stories matching the query parameters across all pages
    /// </summary>
    Task<IEnumerable<Story<T>>> GetAllStoriesAsync<T>(
        StoryQueryParameters parameters,
        string? entity = null,
        CancellationToken cancellationToken = default) where T : class;
}