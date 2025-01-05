using Microsoft.Extensions.Logging;

namespace StoryblokSharp.Http;

/// <summary>
/// Interface for HTTP operations against the Storyblok API
/// </summary>
public interface IStoryblokHttpClient
{
    /// <summary>
    /// Performs a GET request to the specified endpoint
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response to</typeparam>
    /// <param name="endpoint">The API endpoint</param>
    /// <param name="queryParams">Optional query parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The deserialized response</returns>
    Task<T> GetAsync<T>(string endpoint, IDictionary<string, string>? queryParams = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Performs a POST request to the specified endpoint
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response to</typeparam>
    /// <param name="endpoint">The API endpoint</param>
    /// <param name="data">The data to send</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The deserialized response</returns>
    Task<T> PostAsync<T>(string endpoint, object data, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Performs a PUT request to the specified endpoint
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response to</typeparam>
    /// <param name="endpoint">The API endpoint</param>
    /// <param name="data">The data to send</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The deserialized response</returns>
    Task<T> PutAsync<T>(string endpoint, object data, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Performs a DELETE request to the specified endpoint
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response to</typeparam>
    /// <param name="endpoint">The API endpoint</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The deserialized response</returns>
    Task<T> DeleteAsync<T>(string endpoint, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Sets the logger instance for the HTTP client
    /// </summary>
    /// <param name="logger">The logger instance to use for logging HTTP operations</param>
    /// <remarks>
    /// The logger is used to track HTTP requests, responses, and any errors that occur during API operations.
    /// </remarks>
    void SetLogger(ILogger logger);
}