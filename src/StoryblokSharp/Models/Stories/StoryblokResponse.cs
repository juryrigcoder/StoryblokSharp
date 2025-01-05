using System.Text.Json.Serialization;
using StoryblokSharp.Models.Stories;


namespace StoryblokSharp.Models.Stories;
public abstract record StoryblokResponse<T> where T : class
{
    [JsonPropertyName("cv")]
    public required long Cv { get; init; }

    [JsonPropertyName("rels")]
    public Story<T>[] Rels { get; init; } = Array.Empty<Story<T>>();

    [JsonPropertyName("links")]
    public Link[] Links { get; init; } = Array.Empty<Link>();
}