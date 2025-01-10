using Microsoft.Extensions.Logging;

namespace StoryblokSharp.Http;

public abstract partial class StoryblokClientLogging
{
    private ILogger? _logger;

    protected StoryblokClientLogging(ILogger? logger = null)
    {
        _logger = logger;
    }

    public void SetLogger(ILogger logger)
    {
        _logger = logger;
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Making {Method} request to: {Endpoint}")]
    protected static partial void LogRequest(
        ILogger logger,
        string method,
        string endpoint);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Debug,
        Message = "Attempting request (attempt {AttemptNumber})...")]
    protected static partial void LogRequestAttempt(
        ILogger logger,
        int attemptNumber);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Debug,
        Message = "Response Status: {StatusCode}")]
    protected static partial void LogResponseStatus(
        ILogger logger,
        System.Net.HttpStatusCode statusCode);

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Error,
        Message = "HTTP request failed: {Message}")]
    protected static partial void LogHttpRequestError(
        ILogger logger,
        string message,
        Exception? exception = null);

    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Information,
        Message = "Response Content: {Content}")]
    protected static partial void LogResponseContent(
        ILogger logger,
        string content);

    [LoggerMessage(
        EventId = 6,
        Level = LogLevel.Error,
        Message = "JSON Deserialization error: {Message}")]
    protected static partial void LogDeserializationError(
        ILogger logger,
        string message,
        Exception exception);

    [LoggerMessage(
        EventId = 7,
        Level = LogLevel.Warning,
        Message = "Max retries reached for rate limit response")]
    protected static partial void LogMaxRetriesReached(
        ILogger logger);

    [LoggerMessage(
        EventId = 8,
        Level = LogLevel.Information,
        Message = "Rate limited. Waiting {Seconds} seconds before retry...")]
    protected static partial void LogRateLimitWait(
        ILogger logger,
        double seconds);

    [LoggerMessage(
        EventId = 9,
        Level = LogLevel.Warning,
        Message = "Unexpected status code: {StatusCode}")]
    protected static partial void LogUnexpectedStatusCode(
        ILogger logger,
        System.Net.HttpStatusCode statusCode);

    [LoggerMessage(
        EventId = 10,
        Level = LogLevel.Information,
        Message = "Request cancelled by user")]
    protected static partial void LogRequestCancelled(
        ILogger logger);

    [LoggerMessage(
        EventId = 11,
        Level = LogLevel.Error,
        Message = "Max retries reached")]
    protected static partial void LogMaxRetriesReachedError(
        ILogger logger);

    // Helper method to safely log when logger is available
    protected void SafeLog(Action<ILogger> logAction)
    {
        if (_logger != null)
        {
            logAction(_logger);
        }
    }
}