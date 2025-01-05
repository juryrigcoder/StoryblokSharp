using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StoryblokSharp.Exceptions;
using StoryblokSharp.Models.Configuration;

namespace StoryblokSharp.Http;

/// <summary>
/// HTTP client for interacting with the Storyblok Content Delivery API (v2).
/// Handles authentication, request retries, rate limiting, and response handling.
/// </summary>
public class StoryblokHttpClient : IStoryblokHttpClient, IAsyncDisposable
{
    private readonly HttpClient _httpClient;
    private readonly StoryblokOptions _options;
    private readonly JsonSerializerOptions _jsonOptions;
    private bool _disposed;
    private ILogger? _logger;

    /// <summary>
    /// Initializes a new instance of the StoryblokHttpClient.
    /// </summary>
    /// <param name="httpClient">The HttpClient instance used for making HTTP requests.</param>
    /// <param name="options">Configuration options for the Storyblok client.</param>
    /// <exception cref="ArgumentNullException">Thrown when httpClient or options is null.</exception>
    public StoryblokHttpClient(
        HttpClient httpClient,
        IOptions<StoryblokOptions> options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));

        // Set base address directly to api.storyblok.com
        _httpClient.BaseAddress = new Uri("https://api.storyblok.com/v2");
        
        // Keep headers minimal for better performance
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

        // Configure JSON serialization options for proper data handling
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,  // Allow for case-insensitive property matching
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,  // Use camelCase for JSON property names
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,  // Don't serialize null properties
            NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString  // Flexible number handling
        };
    }

    /// <summary>
    /// Configures a logger for the client to enable detailed request/response logging.
    /// </summary>
    /// <param name="logger">The logger instance to use.</param>
    public void SetLogger(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Performs a GET request to the specified endpoint.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response into.</typeparam>
    /// <param name="endpoint">The API endpoint to call.</param>
    /// <param name="queryParams">Optional query parameters to include in the request.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The deserialized response of type T.</returns>
    public async Task<T> GetAsync<T>(
        string endpoint,
        IDictionary<string, string>? queryParams = null,
        CancellationToken cancellationToken = default) where T : class
    {
        // Remove any leading slashes from endpoint for consistency
        endpoint = endpoint.TrimStart('/');

        // Ensure endpoint starts with cdn/stories if it doesn't
        if (!endpoint.StartsWith("cdn/stories", StringComparison.OrdinalIgnoreCase))
        {
            endpoint = $"cdn/stories/{endpoint}";
        }

        // Initialize query parameters and add access token if not provided
        queryParams ??= new Dictionary<string, string>();
        if (!queryParams.ContainsKey("token") && !string.IsNullOrEmpty(_options.AccessToken))
        {
            queryParams["token"] = _options.AccessToken;
        }

        // Ensure there's a trailing slash on the base address
        if (_httpClient.BaseAddress?.ToString().EndsWith('/') == false)
        {
            _httpClient.BaseAddress = new Uri(_httpClient.BaseAddress + "/");
        }

        var url = BuildUrl(endpoint, queryParams);
        _logger?.LogInformation($"Making GET request to: {url}");

        using var response = await SendWithRetryAsync(
            () => _httpClient.GetAsync(url, cancellationToken),
            cancellationToken);

        return await DeserializeResponseAsync<T>(response, cancellationToken);
    }
    
    /// <summary>
    /// Performs a POST request with the provided data.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response into.</typeparam>
    /// <param name="endpoint">The API endpoint to call.</param>
    /// <param name="data">The data to send in the request body.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The deserialized response of type T.</returns>
    public async Task<T> PostAsync<T>(
        string endpoint,
        object data,
        CancellationToken cancellationToken = default) where T : class
    {
        _logger?.LogInformation($"Making POST request to: {endpoint}");
        using var response = await SendWithRetryAsync(
            () => _httpClient.PostAsJsonAsync(endpoint, data, _jsonOptions, cancellationToken),
            cancellationToken);

        return await DeserializeResponseAsync<T>(response, cancellationToken);
    }

    /// <summary>
    /// Performs a PUT request with the provided data.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response into.</typeparam>
    /// <param name="endpoint">The API endpoint to call.</param>
    /// <param name="data">The data to send in the request body.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The deserialized response of type T.</returns>
    public async Task<T> PutAsync<T>(
        string endpoint,
        object data,
        CancellationToken cancellationToken = default) where T : class
    {
        _logger?.LogInformation($"Making PUT request to: {endpoint}");
        using var response = await SendWithRetryAsync(
            () => _httpClient.PutAsJsonAsync(endpoint, data, _jsonOptions, cancellationToken),
            cancellationToken);

        return await DeserializeResponseAsync<T>(response, cancellationToken);
    }

    /// <summary>
    /// Performs a DELETE request to the specified endpoint.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response into.</typeparam>
    /// <param name="endpoint">The API endpoint to call.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The deserialized response of type T.</returns>
    public async Task<T> DeleteAsync<T>(
        string endpoint,
        CancellationToken cancellationToken = default) where T : class
    {
        _logger?.LogInformation($"Making DELETE request to: {endpoint}");
        using var response = await SendWithRetryAsync(
            () => _httpClient.DeleteAsync(endpoint, cancellationToken),
            cancellationToken);

        return await DeserializeResponseAsync<T>(response, cancellationToken);
    }

    /// <summary>
    /// Constructs the complete URL with properly encoded query parameters.
    /// </summary>
    /// <param name="endpoint">The API endpoint.</param>
    /// <param name="queryParams">The query parameters to include.</param>
    /// <returns>The complete URL with encoded query parameters.</returns>
    private static string BuildUrl(string endpoint, IDictionary<string, string> queryParams)
    {
        if (!queryParams.Any())
            return endpoint;

        // Build query string with proper URL encoding for both keys and values
        var queryString = string.Join("&", queryParams
            .Where(p => !string.IsNullOrEmpty(p.Value))
            .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));

        return string.IsNullOrEmpty(queryString)
            ? endpoint
            : $"{endpoint}?{queryString}";
    }

    /// <summary>
    /// Handles request execution with retry logic and rate limiting support.
    /// </summary>
    /// <param name="request">The request to execute.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The HTTP response message.</returns>
    private async Task<HttpResponseMessage> SendWithRetryAsync(
        Func<Task<HttpResponseMessage>> request,
        CancellationToken cancellationToken)
    {
        var retryCount = 0;
        var delay = TimeSpan.FromSeconds(1);

        while (true)
        {
            try
            {
                _logger?.LogDebug($"Attempting request (attempt {retryCount + 1})...");
                var response = await request();
                _logger?.LogDebug($"Response Status: {response.StatusCode}");

                // Apply response interceptor if configured
                if (_options.ResponseInterceptor != null)
                {
                    _logger?.LogDebug("Applying response interceptor...");
                    response = await _options.ResponseInterceptor(response);
                }

                if (response.IsSuccessStatusCode)
                {
                    _logger?.LogDebug("Request successful!");
                    return response;
                }

                // Handle rate limiting (429 Too Many Requests)
                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    if (retryCount >= _options.MaxRetries)
                    {
                        _logger?.LogWarning("Max retries reached for rate limit response");
                        throw new StoryblokApiException(response);
                    }

                    // Use the Retry-After header if provided
                    if (response.Headers.RetryAfter?.Delta is not null)
                    {
                        delay = response.Headers.RetryAfter.Delta.Value;
                        _logger?.LogInformation($"Rate limited. Waiting {delay.TotalSeconds} seconds before retry...");
                        await Task.Delay(delay, cancellationToken);
                        retryCount++;
                        continue;
                    }

                    throw new StoryblokApiException(response);
                }

                _logger?.LogWarning($"Unexpected status code: {response.StatusCode}");
                throw new StoryblokApiException(response);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger?.LogInformation("Request cancelled by user");
                throw;
            }
            catch (HttpRequestException ex)
            {
                _logger?.LogError($"HTTP request failed: {ex.Message}");
                if (retryCount >= _options.MaxRetries)
                {
                    _logger?.LogError("Max retries reached");
                    throw new StoryblokApiException("Request failed after maximum retries", ex);
                }

                _logger?.LogInformation($"Waiting {delay.TotalSeconds} seconds before retry...");
                await Task.Delay(delay, cancellationToken);
                retryCount++;
            }
        }
    }

    /// <summary>
    /// Deserializes the HTTP response content into the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize into.</typeparam>
    /// <param name="response">The HTTP response message.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The deserialized object of type T.</returns>
    /// <exception cref="StoryblokApiException">Thrown when deserialization fails or results in null.</exception>
    private async Task<T> DeserializeResponseAsync<T>(
        HttpResponseMessage response,
        CancellationToken cancellationToken) where T : class
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger?.LogInformation($"Response Content: {content}");

        try
        {
            var result = JsonSerializer.Deserialize<T>(content, _jsonOptions);
            if (result == null)
            {
                _logger?.LogError("Deserialization resulted in null object");
                throw new StoryblokApiException("Response was null");
            }
            _logger?.LogDebug($"Successfully deserialized to type {typeof(T).Name}");
            return result;
        }
        catch (JsonException ex)
        {
            _logger?.LogError($"JSON Deserialization error: {ex.Message}");
            _logger?.LogError($"Content that failed to deserialize: {content}");
            throw new StoryblokApiException("Failed to deserialize response", ex);
        }
    }

    /// <summary>
    /// Disposes of the HTTP client resources.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}