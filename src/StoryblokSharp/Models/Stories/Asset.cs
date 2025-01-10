using System.Text.Json.Serialization;
using StoryblokSharp.Models.Common;
using StoryblokSharp.Models.Json;

namespace StoryblokSharp.Models.Stories;

/// <summary>
/// Represents an asset in Storyblok
/// </summary>
public record Asset : StoryblokEntity
{
    /// <summary>
    /// Alternative text
    /// </summary>
    [JsonPropertyName("alt")]
    public string? Alt { get; init; }

    /// <summary>
    /// Focus point
    /// </summary>
    [JsonPropertyName("focus")]
    public string? Focus { get; init; }

    /// <summary>
    /// Asset title
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; init; }

    /// <summary>
    /// Asset filename/URL
    /// </summary>
    [JsonPropertyName("filename")]
    public required string Filename { get; init; }

    /// <summary>
    /// Copyright information
    /// </summary>
    [JsonPropertyName("copyright")]
    public string? Copyright { get; init; }

    /// <summary>
    /// Additional metadata
    /// </summary>
    [JsonPropertyName("meta_data")]
    public Dictionary<string, object>? MetaData { get; init; }
}