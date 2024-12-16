using System.Text.Json.Serialization;

namespace StoryblokSharp.Models.Stories;

/// <summary>
/// Response containing multiple stories
/// </summary>
public record StoriesResponse<T> where T : class
{
    /// <summary>
    /// Collection of stories
    /// </summary>
    [JsonPropertyName("stories")]
    public required Story<T>[] Stories { get; set; }

    /// <summary>
    /// Cache version
    /// </summary>
    [JsonPropertyName("cv")]
    public required long Cv { get; set; }

    /// <summary>
    /// Related stories
    /// </summary>
    [JsonPropertyName("rels")]
    public required Story<T>[] Rels { get; set; } = Array.Empty<Story<T>>();

    /// <summary>
    /// Story links
    /// </summary>
    [JsonPropertyName("links")]
    public required Link[] Links { get; set; } = Array.Empty<Link>();

}