using System.Text.Json.Serialization;

namespace StoryblokSharp.Models.Stories;

/// <summary>
/// Represents a rich text field in Storyblok
/// </summary>
public record RichTextField
{
    /// <summary>
    /// Type of the field
    /// </summary>
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    /// <summary>
    /// Rich text content
    /// </summary>
    [JsonPropertyName("content")]
    public RichTextField[]? Content { get; init; }

    /// <summary>
    /// Marks applied to the text
    /// </summary>
    [JsonPropertyName("marks")]
    public RichTextMark[]? Marks { get; init; }

    /// <summary>
    /// Text content
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; init; }

    /// <summary>
    /// Additional attributes
    /// </summary>
    [JsonPropertyName("attrs")]
    public Dictionary<string, object>? Attrs { get; init; }
}

/// <summary>
/// Represents a mark in rich text
/// </summary>
public record RichTextMark
{
    /// <summary>
    /// Type of mark
    /// </summary>
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    /// <summary>
    /// Mark attributes
    /// </summary>
    [JsonPropertyName("attrs")]
    public Dictionary<string, object>? Attrs { get; init; }
}