using System.Text.Json.Serialization;
using StoryblokSharp.Models.Common;

namespace StoryblokSharp.Models.Stories;

/// <summary>
/// Represents a Storyblok story response
/// </summary>
public record StoryResponse<T> : StoryblokResponse<T> where T : class
{
    [JsonPropertyName("story")]
    public required Story<T> Story { get; init; }
}

/// <summary>
/// Represents a single Storyblok story
/// </summary>
public record Story<T> : StoryblokEntity where T : class
{
    /// <summary>
    /// Story slug
    /// </summary>
    [JsonPropertyName("slug")]
    public required string Slug { get; init; }

    /// <summary>
    /// Full slug including parent folders
    /// </summary>
    [JsonPropertyName("full_slug")]
    public required string FullSlug { get; init; }

    /// <summary>
    /// Story content as strongly typed object
    /// </summary>
    [JsonPropertyName("content")]
    public required T Content { get; init; }

    /// <summary>
    /// Created date
    /// </summary>
    [JsonPropertyName("created_at")]
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// First published date
    /// </summary>
    [JsonPropertyName("first_published_at")]
    public DateTime? FirstPublishedAt { get; init; }

    /// <summary>
    /// Published date
    /// </summary>
    [JsonPropertyName("published_at")]
    public DateTime? PublishedAt { get; init; }

    /// <summary>
    /// Last updated date
    /// </summary>
    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; init; }

    /// <summary>
    /// Parent ID if story is nested
    /// </summary>  
    [JsonPropertyName("parent_id")]
    public long? ParentId { get; init; }

    /// <summary>
    /// Indicates if story is startpage of folder
    /// </summary>
    [JsonPropertyName("is_startpage")]
    public bool IsStartpage { get; init; }

    /// <summary>
    /// Story position in navigation
    /// </summary>
    [JsonPropertyName("position")]
    public int Position { get; init; }

    /// <summary>
    /// List of tags
    /// </summary>
    [JsonPropertyName("tag_list")]
    public string[] TagList { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Group ID
    /// </summary>
    [JsonPropertyName("group_id")]
    public string? GroupId { get; init; }

    /// <summary>
    /// Language code
    /// </summary>
    [JsonPropertyName("lang")]
    public string? Lang { get; init; }

    /// <summary>
    /// Additional metadata
    /// </summary>
    [JsonPropertyName("meta_data")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? MetaData { get; init; }
}

/// <summary>
/// Base content type for story content that provides common Storyblok properties
/// </summary>
public record StoryblokComponent
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    [JsonPropertyName("_uid")]
    public required string Uid { get; init; }

    /// <summary>
    /// Component type name
    /// </summary>
    [JsonPropertyName("component")]
    public required string Component { get; init; }
}

/// <summary>
/// Represents story links
/// </summary>
public record Link : StoryblokEntity
{
    /// <summary>
    /// Link slug
    /// </summary>
    [JsonPropertyName("slug")]
    public required string Slug { get; init; }

    /// <summary>
    /// Parent ID if nested
    /// </summary>
    [JsonPropertyName("parent_id")]
    public long? ParentId { get; init; }
}