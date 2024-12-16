using StoryblokSharp.Models.Stories;

namespace StoryblokSharp.Services.Stories;

/// <summary>
/// Extension methods for stories
/// </summary>
public static class StoryExtensions 
{
    /// <summary>
    /// Checks if a story has unpublished changes
    /// </summary>
    public static bool HasUnpublishedChanges<T>(this Story<T> story) where T : class
        => story.PublishedAt != story.UpdatedAt;

    /// <summary>
    /// Gets the full path including parent slugs
    /// </summary>
    public static string GetFullPath<T>(this Story<T> story) where T : class
        => story.FullSlug.TrimStart('/');

    /// <summary>
    /// Gets the parent slug if story is nested
    /// </summary>
    public static string? GetParentSlug<T>(this Story<T> story) where T : class
    {
        var parts = story.FullSlug.TrimStart('/').Split('/');
        return parts.Length > 1 ? string.Join("/", parts.Take(parts.Length - 1)) : null;
    }
}