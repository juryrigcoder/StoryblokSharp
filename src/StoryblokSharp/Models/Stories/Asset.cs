using System.Text.Json.Serialization;
using StoryblokSharp.Models.Json;

namespace StoryblokSharp.Models.Stories;

/// <summary>
/// Represents an asset in Storyblok
/// </summary>
public record Asset
{
    /// <summary>
    /// Asset ID
    /// </summary>
    [JsonPropertyName("id")]
    [JsonConverter(typeof(FlexibleNumberConverter))]
    public required long Id { get; init; }

    /// <summary>
    /// Alternative text
    /// </summary>
    [JsonPropertyName("alt")]
    public string? Alt { get; init; }

    /// <summary>
    /// Asset name
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

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