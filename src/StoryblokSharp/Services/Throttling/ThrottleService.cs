using System.Collections.Concurrent;

namespace StoryblokSharp.Services.Throttling;

/// <summary>
/// Default implementation of IThrottleService that limits request rate
/// </summary>
public class ThrottleService : IThrottleService
{
    private readonly ConcurrentQueue<DateTimeOffset> _requestTimestamps;
    private readonly SemaphoreSlim _semaphore;
    private int _requestsPerSecond;
    private readonly object _rateLimitLock = new();

    public ThrottleService(int initialRequestsPerSecond = 5)
    {
        _requestTimestamps = new ConcurrentQueue<DateTimeOffset>();
        _semaphore = new SemaphoreSlim(1);
        _requestsPerSecond = initialRequestsPerSecond;
    }

    /// <inheritdoc/>
    public void SetRateLimit(int requestsPerSecond)
    {
        if (requestsPerSecond <= 0)
            throw new ArgumentOutOfRangeException(nameof(requestsPerSecond), "Rate limit must be greater than zero");

        lock (_rateLimitLock)
        {
            _requestsPerSecond = requestsPerSecond;
        }
    }

    /// <inheritdoc/>
    public async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> action,
        CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            await WaitForRateLimitWindowAsync(cancellationToken);
            _requestTimestamps.Enqueue(DateTimeOffset.UtcNow);
            
            return await action(cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task WaitForRateLimitWindowAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            // Remove expired timestamps
            var now = DateTimeOffset.UtcNow;
            while (_requestTimestamps.TryPeek(out var timestamp))
            {
                if ((now - timestamp).TotalSeconds >= 1)
                    _requestTimestamps.TryDequeue(out _);
                else
                    break;
            }

            // Check if we can make a new request
            if (_requestTimestamps.Count < _requestsPerSecond)
                break;

            // Wait a bit before checking again
            await Task.Delay(100, cancellationToken);
        }
    }
}