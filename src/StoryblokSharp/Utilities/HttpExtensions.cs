namespace StoryblokSharp.Utilities;

/// <summary>
/// Extensions for HTTP operations
/// </summary>
public static class HttpExtensions
{
    /// <summary>
    /// Executes a request with retry logic
    /// </summary>
    public static async Task<HttpResponseMessage> ExecuteWithRetryAsync(
        this HttpClient client,
        Func<Task<HttpResponseMessage>> action,
        int maxRetries = 3,
        int baseDelayMs = 1000,
        CancellationToken cancellationToken = default)
    {
        Exception? lastException = null;

        for (var i = 0; i < maxRetries; i++)
        {
            try
            {
                var response = await action();
                
                if ((int)response.StatusCode != 429)
                    return response;

                var retryAfter = response.Headers.RetryAfter?.Delta 
                    ?? TimeSpan.FromMilliseconds(baseDelayMs * Math.Pow(2, i));
                
                await Task.Delay(retryAfter, cancellationToken);
            }
            catch (Exception ex)
            {
                lastException = ex;
                
                if (i == maxRetries - 1)
                    throw new HttpRequestException(
                        "Request failed after maximum retries", 
                        lastException);

                await Task.Delay(
                    TimeSpan.FromMilliseconds(baseDelayMs * Math.Pow(2, i)),
                    cancellationToken);
            }
        }

        throw new HttpRequestException(
            "Request failed after maximum retries",
            lastException);
    }
}