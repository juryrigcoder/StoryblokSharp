using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using StoryblokSharp.Exceptions;
using StoryblokSharp.Models.Configuration;

namespace StoryblokSharp.Http;

public class StoryblokHttpClient : IStoryblokHttpClient, IAsyncDisposable
{
    private readonly HttpClient _httpClient;
    private readonly StoryblokOptions _options;
    private readonly JsonSerializerOptions _jsonOptions;
    private bool _disposed;

    public StoryblokHttpClient(
        HttpClient httpClient,
        IOptions<StoryblokOptions> options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));

        // Set base address directly to api.storyblok.com
        _httpClient.BaseAddress = new Uri("https://api.storyblok.com/v2");
        
        // Keep headers minimal
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

        // Configure JSON options
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString
        };
    }

public async Task<T> GetAsync<T>(
    string endpoint,
    IDictionary<string, string>? queryParams = null,
    CancellationToken cancellationToken = default) where T : class
{
    // Remove any leading slashes from endpoint
    endpoint = endpoint.TrimStart('/');

    // Ensure endpoint starts with cdn/stories if it doesn't
    if (!endpoint.StartsWith("cdn/stories", StringComparison.OrdinalIgnoreCase))
    {
        endpoint = $"cdn/stories/{endpoint}";
    }

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

    using var response = await SendWithRetryAsync(
        () => _httpClient.GetAsync(url, cancellationToken),
        cancellationToken);

    return await DeserializeResponseAsync<T>(response, cancellationToken);
}
    
    
    public async Task<T> PostAsync<T>(
        string endpoint,
        object data,
        CancellationToken cancellationToken = default) where T : class
    {
        using var response = await SendWithRetryAsync(
            () => _httpClient.PostAsJsonAsync(endpoint, data, _jsonOptions, cancellationToken),
            cancellationToken);

        return await DeserializeResponseAsync<T>(response, cancellationToken);
    }

    public async Task<T> PutAsync<T>(
        string endpoint,
        object data,
        CancellationToken cancellationToken = default) where T : class
    {
        using var response = await SendWithRetryAsync(
            () => _httpClient.PutAsJsonAsync(endpoint, data, _jsonOptions, cancellationToken),
            cancellationToken);

        return await DeserializeResponseAsync<T>(response, cancellationToken);
    }

    public async Task<T> DeleteAsync<T>(
        string endpoint,
        CancellationToken cancellationToken = default) where T : class
    {
        using var response = await SendWithRetryAsync(
            () => _httpClient.DeleteAsync(endpoint, cancellationToken),
            cancellationToken);

        return await DeserializeResponseAsync<T>(response, cancellationToken);
    }

    private static string BuildUrl(string endpoint, IDictionary<string, string> queryParams)
    {
        if (!queryParams.Any())
            return endpoint;

        var queryString = string.Join("&", queryParams
            .Where(p => !string.IsNullOrEmpty(p.Value))
            .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));

        return string.IsNullOrEmpty(queryString)
            ? endpoint
            : $"{endpoint}?{queryString}";
    }

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
                //Console.WriteLine($"Attempting request (attempt {retryCount + 1})...");
                var response = await request();
                //Console.WriteLine($"Response Status: {response.StatusCode}");

                if (_options.ResponseInterceptor != null)
                {
                    //Console.WriteLine("Applying response interceptor...");
                    response = await _options.ResponseInterceptor(response);
                }

                if (response.IsSuccessStatusCode)
                {
                    //Console.WriteLine("Request successful!");
                    return response;
                }

                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    if (retryCount >= _options.MaxRetries)
                    {
                        //Console.WriteLine("Max retries reached for rate limit response");
                        throw new StoryblokApiException(response);
                    }

                    if (response.Headers.RetryAfter?.Delta is not null)
                    {
                        delay = response.Headers.RetryAfter.Delta.Value;
                        //Console.WriteLine($"Rate limited. Waiting {delay.TotalSeconds} seconds before retry...");
                        await Task.Delay(delay, cancellationToken);
                        retryCount++;
                        continue;
                    }

                    throw new StoryblokApiException(response);
                }

                //Console.WriteLine($"Unexpected status code: {response.StatusCode}");
                throw new StoryblokApiException(response);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                //Console.WriteLine("Request cancelled by user");
                throw;
            }
            catch (HttpRequestException ex)
            {
                //Console.WriteLine($"HTTP request failed: {ex.Message}");
                if (retryCount >= _options.MaxRetries)
                {
                    //Console.WriteLine("Max retries reached");
                    throw new StoryblokApiException("Request failed after maximum retries", ex);
                }

                //Console.WriteLine($"Waiting {delay.TotalSeconds} seconds before retry...");
                await Task.Delay(delay, cancellationToken);
                retryCount++;
            }
        }
    }

    private async Task<T> DeserializeResponseAsync<T>(
        HttpResponseMessage response,
        CancellationToken cancellationToken) where T : class
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        Console.WriteLine($"Response Content: {content}");  // Debug line

        try
        {
            return JsonSerializer.Deserialize<T>(content, _jsonOptions)
                ?? throw new StoryblokApiException("Response was null");
        }
        catch (JsonException ex)
        {
            throw new StoryblokApiException("Failed to deserialize response", ex);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}