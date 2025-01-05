using System.Text.Json.Serialization;

namespace StoryblokSharp.Models.Stories;

/// <summary>
/// Response containing multiple stories
/// </summary>
public record StoriesResponse<T> : StoryblokResponse<T> where T : class
{
    [JsonPropertyName("stories")]
    public required Story<T>[] Stories { get; init; }
}