namespace StoryblokSharp.Models;

public record StoryblokOptions
{
    public required string AccessToken { get; init; }
    public string? OAuthToken { get; init; }
    public Region Region { get; init; } = Region.EU;
    public bool UseHttps { get; init; } = true;
    public int MaxRetries { get; init; } = 5;
    public TimeSpan RetryDelay { get; init; } = TimeSpan.FromSeconds(1);
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(30);
    public int RateLimit { get; init; } = 5;
    public CacheOptions Cache { get; init; } = new();
}

public record CacheOptions
{
    public CacheType Type { get; init; } = CacheType.Memory;
    public CacheClearMode ClearMode { get; init; } = CacheClearMode.Manual;
    public TimeSpan? DefaultExpiration { get; init; }
}

public enum CacheType
{
    None,
    Memory,
    Custom
}

public enum CacheClearMode
{
    Manual,
    Auto
}

public enum Region
{
    EU,
    US,
    China,
    AsiaPacific,
    Canada
}

public enum ApiVersion
{
    V1,
    V2
}