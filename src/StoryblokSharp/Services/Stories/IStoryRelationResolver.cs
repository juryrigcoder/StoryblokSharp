using StoryblokSharp.Models.Stories;

namespace StoryblokSharp.Services.Stories;

/// <summary>
/// Service for resolving story relations and links
/// </summary>
public interface IStoryRelationResolver
{
    /// <summary>
    /// Resolves relations and links in story content
    /// </summary>
    Task ResolveRelationsAsync<T>(
        StoryResponse<T> response,
        StoryQueryParameters parameters,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Resolves links in story content
    /// </summary> 
    Task ResolveLinksAsync<T>(
        StoryResponse<T> response,
        StoryQueryParameters parameters,
        CancellationToken cancellationToken = default) where T : class;
}