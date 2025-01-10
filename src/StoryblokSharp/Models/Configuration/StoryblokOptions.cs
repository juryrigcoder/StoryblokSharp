using StoryblokSharp.Services.RichText;
using StoryblokSharp.Models.Common;
using StoryblokSharp.Models.Cache;

namespace StoryblokSharp.Models.Configuration;

/// <summary>
/// Configuration options for the Storyblok client
/// </summary>
public record StoryblokOptions
{
    /// <summary>
    /// The API access token for content delivery API (v2)
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>
    /// The OAuth token for management API (v1)
    /// </summary>
    public string? OAuthToken { get; set; }

    /// <summary>
    /// Whether to resolve nested relations
    /// </summary>
    public bool ResolveNestedRelations { get; set; } = true;

    /// <summary>
    /// Cache configuration
    /// </summary>
    public CacheOptions Cache { get; set; } = new();

    /// <summary>
    /// Custom HTTP response interceptor
    /// </summary>
    public Func<HttpResponseMessage, Task<HttpResponseMessage>>? ResponseInterceptor { get; set; }

    /// <summary>
    /// Request timeout in seconds
    /// </summary>
    public int Timeout { get; set; } = 0;

    /// <summary>
    /// Custom headers to include in requests
    /// </summary>
    public IDictionary<string, string>? Headers { get; set; }

    /// <summary>
    /// API region
    /// </summary>
    public Region Region { get; set; } = Region.EU;

    /// <summary>
    /// Maximum number of retry attempts
    /// </summary>
    public int MaxRetries { get; set; } = 10;

    /// <summary>
    /// Whether to use HTTPS
    /// </summary>
    public bool UseHttps { get; set; } = true;

    /// <summary>
    /// Rate limit for requests per second
    /// </summary>
    public int RateLimit { get; set; } = 5;

    /// <summary>
    /// Custom endpoint URL
    /// </summary>
    public string? Endpoint { get; set; }

    /// <summary>
    /// Custom schema for rich text rendering
    /// </summary>
    public IRichTextSchema? RichTextSchema { get; set; }

    /// <summary>
    /// Component resolver for custom components
    /// </summary>
    public Func<string, object, string>? ComponentResolver { get; set; }
}