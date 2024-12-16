using System.Text.Json.Serialization;

namespace StoryblokSharp.Models.Stories;

/// <summary>
/// Parameters for querying stories
/// </summary>
public record StoryQueryParameters
{
    /// <summary>
    /// Content version (draft/published)
    /// </summary>
    [JsonPropertyName("version")]
    public string? Version { get; init; }

    /// <summary>
    /// Language code
    /// </summary>
    [JsonPropertyName("language")]
    public string? Language { get; init; }

    /// <summary>
    /// Component relations to resolve
    /// </summary>
    [JsonPropertyName("resolve_relations")]
    public string[]? ResolveRelations { get; init; }

    /// <summary>
    /// How to resolve links
    /// </summary>
    [JsonPropertyName("resolve_links")]
    public string? ResolveLinks { get; init; }

    /// <summary>
    /// Cache version
    /// </summary>
    [JsonPropertyName("cv")]
    public string? Cv { get; init; }

    /// <summary>
    /// Page number for pagination
    /// </summary>
    [JsonPropertyName("page")]
    public int Page { get; init; } = 1;

    /// <summary>
    /// Items per page
    /// </summary>
    [JsonPropertyName("per_page")]
    public int PerPage { get; init; } = 25;

    /// <summary>
    /// Search term
    /// </summary>
    [JsonPropertyName("search_term")]
    public string? SearchTerm { get; init; }

    /// <summary>
    /// Sort order
    /// </summary>
    [JsonPropertyName("sort_by")]
    public string? SortBy { get; init; }

    /// <summary>
    /// Filter by tag
    /// </summary>
    [JsonPropertyName("with_tag")]
    public string? WithTag { get; init; }

    /// <summary>
    /// Exclude fields
    /// </summary>
    [JsonPropertyName("excluding_fields")]
    public string? ExcludingFields { get; init; }

    /// <summary>
    /// By UUIDs ordered
    /// </summary>
    [JsonPropertyName("by_uuids_ordered")]
    public string? ByUuidsOrdered { get; init; }

    /// <summary>
    /// By UUIDs
    /// </summary>
    [JsonPropertyName("by_uuids")]
    public string? ByUuids { get; init; }

    /// <summary>
    /// Filter by slugs
    /// </summary>
    [JsonPropertyName("by_slugs")]
    public string? BySlugs { get; init; }

    /// <summary>
    /// Filter starting with path
    /// </summary>
    [JsonPropertyName("starts_with")]
    public string? StartsWith { get; init; }

    /// <summary>
    /// Access token
    /// </summary>
    [JsonPropertyName("token")]
    public string? Token { get; set; }

    /// <summary>
    /// Resolve level for nested content
    /// </summary>
    [JsonPropertyName("resolve_level")]
    public int? ResolveLevel { get; init; }

    /// <summary>
    /// Fallback language
    /// </summary>
    [JsonPropertyName("fallback_lang")]
    public string? FallbackLang { get; init; }
}