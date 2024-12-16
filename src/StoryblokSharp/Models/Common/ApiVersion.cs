namespace StoryblokSharp.Models.Common;

/// <summary>
/// Represents the Storyblok API version
/// </summary>
public enum ApiVersion
{
    /// <summary>
    /// Version 1 - Used for management API (OAuth token)
    /// </summary>
    V1,

    /// <summary>
    /// Version 2 - Used for content delivery API (access token)
    /// </summary>
    V2
}

/// <summary>
/// Extension methods for ApiVersion enum
/// </summary>
public static class ApiVersionExtensions
{
    /// <summary>
    /// Converts the API version to its string representation
    /// </summary>
    /// <param name="version">The API version</param>
    /// <returns>The string representation of the API version (e.g., "v1" or "v2")</returns>
    public static string ToVersionString(this ApiVersion version) => version.ToString().ToLowerInvariant();

    /// <summary>
    /// Returns whether this API version is for the management API
    /// </summary>
    /// <param name="version">The API version</param>
    /// <returns>True if this is a management API version, false otherwise</returns>
    public static bool IsManagementVersion(this ApiVersion version) => version == ApiVersion.V1;

    /// <summary>
    /// Returns whether this API version is for the content delivery API
    /// </summary>
    /// <param name="version">The API version</param>
    /// <returns>True if this is a content delivery API version, false otherwise</returns>
    public static bool IsContentDeliveryVersion(this ApiVersion version) => version == ApiVersion.V2;
}