namespace StoryblokSharp.Models.Common;

/// <summary>
/// Represents the available Storyblok API regions
/// </summary>
public enum Region
{
    /// <summary>
    /// European region (Default) - api.storyblok.com
    /// </summary>
    EU,

    /// <summary>
    /// United States region - api-us.storyblok.com
    /// </summary>
    US,

    /// <summary>
    /// China region - app.storyblokchina.cn
    /// </summary>
    China,

    /// <summary>
    /// Asia Pacific region - api-ap.storyblok.com
    /// </summary>
    AsiaPacific,

    /// <summary>
    /// Canada region - api-ca.storyblok.com
    /// </summary>
    Canada
}

/// <summary>
/// Extension methods for Region enum
/// </summary>
public static class RegionExtensions
{
    private const string EU_API_URL = "api.storyblok.com";
    private const string US_API_URL = "api-us.storyblok.com";
    private const string CN_API_URL = "app.storyblokchina.cn";
    private const string AP_API_URL = "api-ap.storyblok.com";
    private const string CA_API_URL = "api-ca.storyblok.com";

    /// <summary>
    /// Gets the base URL for the specified region
    /// </summary>
    /// <param name="region">The region</param>
    /// <returns>The base URL for the region</returns>
    public static string GetBaseUrl(this Region region) => region switch
    {
        Region.US => US_API_URL,
        Region.China => CN_API_URL,
        Region.AsiaPacific => AP_API_URL,
        Region.Canada => CA_API_URL,
        _ => EU_API_URL
    };

    /// <summary>
    /// Gets the full URL for the specified region, including protocol and API version
    /// </summary>
    /// <param name="region">The region</param>
    /// <param name="useHttps">Whether to use HTTPS protocol</param>
    /// <param name="version">The API version to use</param>
    /// <returns>The complete URL for the region</returns>
    public static string GetEndpoint(this Region region, bool useHttps = true, ApiVersion version = ApiVersion.V2)
    {
        var protocol = useHttps ? "https" : "http";
        var apiVersion = version == ApiVersion.V1 ? "v1" : "v2";
        return $"{protocol}://{region.GetBaseUrl()}/{apiVersion}";
    }
}
