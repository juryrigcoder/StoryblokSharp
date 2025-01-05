using System.Text.Json.Serialization;

namespace StoryblokSharp.Models.Common;

/// <summary>
/// Represents a base entity in the Storyblok content management system.
/// This abstract record provides common properties shared across all Storyblok entities.
/// </summary>
public abstract record StoryblokEntity
{
    /// <summary>
    /// Gets the numeric identifier of the entity.
    /// This is a unique identifier assigned by Storyblok.
    /// </summary>
    [JsonPropertyName("id")]
    public required long Id { get; init; }

    /// <summary>
    /// Gets the UUID (Universally Unique Identifier) of the entity.
    /// This is a unique string identifier used to reference the entity across the Storyblok system.
    /// </summary>
    [JsonPropertyName("uuid")]
    public required string Uuid { get; init; }

    /// <summary>
    /// Gets the display name of the entity.
    /// This is the human-readable name used to identify the entity in the Storyblok interface.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }
}