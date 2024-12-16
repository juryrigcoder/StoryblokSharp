namespace StoryblokSharp.Services.Throttling;

/// <summary>
/// Service for throttling API requests to stay within rate limits
/// </summary>
public interface IThrottleService
{
    /// <summary>
    /// Executes the provided action while respecting rate limits
    /// </summary>
    /// <typeparam name="T">The type of the result</typeparam>
    /// <param name="action">The action to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the action</returns>
    Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the rate limit in requests per second
    /// </summary>
    /// <param name="requestsPerSecond">The maximum number of requests allowed per second</param>
    void SetRateLimit(int requestsPerSecond);
}