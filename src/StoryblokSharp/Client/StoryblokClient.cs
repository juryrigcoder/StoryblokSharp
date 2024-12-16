using Microsoft.Extensions.Options;
using StoryblokSharp.Models.Configuration;
using StoryblokSharp.Models.Stories;
using StoryblokSharp.Services.Cache;
using StoryblokSharp.Services.Throttling;
using StoryblokSharp.Http;
using StoryblokSharp.Models.RichText;
using System.Globalization;

namespace StoryblokSharp.Client;

/// <summary>
/// Main client for interacting with the Storyblok API
/// </summary>
public sealed class StoryblokClient : IStoryblokClient
{
    private readonly IStoryblokHttpClient _httpClient;
    private readonly IStoryblokCache _cache;
    private readonly IThrottleService _throttle;
    private readonly IRichTextRenderer _richTextRenderer;
    private readonly StoryblokOptions _options;
    private readonly IDictionary<string, int> _cacheVersions;
    private bool _disposed;

    private const int DEFAULT_PAGE_SIZE = 25;
    private const int MAX_PAGE_SIZE = 100;

    public StoryblokClient(
        IStoryblokHttpClient httpClient,
        IStoryblokCache cache,
        IThrottleService throttle,
        IRichTextRenderer richTextRenderer,
        IOptions<StoryblokOptions> options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _throttle = throttle ?? throw new ArgumentNullException(nameof(throttle));
        _richTextRenderer = richTextRenderer ?? throw new ArgumentNullException(nameof(richTextRenderer));
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        _cacheVersions = new Dictionary<string, int>();
        

        if (_options.RateLimit > 0)
            _throttle.SetRateLimit(_options.RateLimit);

        if (_options.ComponentResolver != null)
        {
            _richTextRenderer.AddNode("blok", node => new SchemaNode
            {
                Html = node.Attrs != null && node.Attrs.TryGetValue("body", out var body) && body is object[] bodyArray
                    ? string.Join("", bodyArray
                        .Cast<IDictionary<string, object>>()
                        .Select(blok => _options.ComponentResolver(
                            blok["component"].ToString()!,
                            blok)))
                    : string.Empty
            });
        }
    }

    public async Task<StoryResponse<T>> GetStoryAsync<T>(
        string slug,
        StoryQueryParameters? parameters = null,
        CancellationToken cancellationToken = default) where T : class
    {
        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentNullException(nameof(slug), "Story slug cannot be null or whitespace.");

        parameters = EnsureValidParameters(parameters ?? new StoryQueryParameters());

        var queryParams = BuildQueryParameters(parameters, false);
        var endpoint = $"/cdn/stories/{slug.TrimStart('/')}";
        var cacheKey = GenerateCacheKey(endpoint, queryParams);

        if (parameters.Version != "draft")
        {
            var cached = await TryGetFromCache<StoryResponse<T>>(cacheKey, cancellationToken);
            if (cached != null)
                return cached;
        }

        return await _throttle.ExecuteAsync(async token =>
        {
            var response = await _httpClient.GetAsync<StoryResponse<T>>(endpoint, queryParams, token);

            if (parameters.Version != "draft")
            {
                await CacheResponse(cacheKey, response, parameters.Token!, token);
            }

            return response;
        }, cancellationToken);
    }
    public async Task<StoriesResponse<T>> GetStoriesAsync<T>(
        StoryQueryParameters parameters,
        CancellationToken cancellationToken = default) where T : class
    {
        ArgumentNullException.ThrowIfNull(parameters);

        parameters = EnsureValidParameters(parameters);
        var queryParams = BuildQueryParameters(parameters, true); // isMultipleStories = true
        var cacheKey = GenerateCacheKey("cdn/stories", queryParams);

        if (parameters.Version != "draft")
        {
            var cached = await TryGetFromCache<StoriesResponse<T>>(cacheKey, cancellationToken);
            if (cached != null)
                return cached;
        }

        return await _throttle.ExecuteAsync(async token =>
        {
            var response = await _httpClient.GetAsync<StoriesResponse<T>>("cdn/stories", queryParams, token);

            if (parameters.Version != "draft")
            {
                await CacheResponse(cacheKey, response, parameters.Token!, token);
            }

            return response;
        }, cancellationToken);
    }

    public async Task<IEnumerable<Story<T>>> GetAllAsync<T>(
        string endpoint,
        StoryQueryParameters parameters,
        string? entity = null,
        CancellationToken cancellationToken = default) where T : class
    {
        if (string.IsNullOrEmpty(endpoint))
            throw new ArgumentNullException(nameof(endpoint));
        ArgumentNullException.ThrowIfNull(parameters);

        parameters = EnsureValidParameters(parameters);
        var perPage = parameters.PerPage > 0 ? Math.Min(parameters.PerPage, MAX_PAGE_SIZE) : DEFAULT_PAGE_SIZE;

        // Clean the endpoint
        endpoint = endpoint.TrimEnd('/');

        // Get first page
        var firstPage = await MakeRequestAsync<T>(endpoint, parameters, perPage, 1, cancellationToken);
        var totalPages = firstPage.Stories.Length > 0 ? (int)Math.Ceiling(firstPage.Stories.Length / (double)perPage) : 1;

        var results = new List<Story<T>>();
        results.AddRange(firstPage.Stories);

        // Get remaining pages in parallel
        if (totalPages > 1)
        {
            var remainingPages = await Task.WhenAll(
                Enumerable.Range(2, totalPages - 1)
                    .Select(page => MakeRequestAsync<T>(endpoint, parameters, perPage, page, cancellationToken))
            );

            foreach (var pageResult in remainingPages)
            {
                results.AddRange(pageResult.Stories);
            }
        }

        return results;
    }

    private async Task<StoriesResponse<T>> MakeRequestAsync<T>(
        string endpoint,
        StoryQueryParameters parameters,
        int perPage,
        int page,
        CancellationToken cancellationToken) where T : class
    {
        var pagedParameters = parameters with { PerPage = perPage, Page = page };
        var queryParams = BuildQueryParameters(pagedParameters, true); // isMultipleStories = true

        return await _throttle.ExecuteAsync(async token =>
        {
            var response = await _httpClient.GetAsync<StoriesResponse<T>>(endpoint, queryParams, token);

            if (parameters.Version != "draft")
            {
                var cacheKey = GenerateCacheKey(endpoint, queryParams);
                await CacheResponse(cacheKey, response, parameters.Token!, token);
            }

            return response;
        }, cancellationToken);
    }

    private StoryQueryParameters EnsureValidParameters(StoryQueryParameters parameters)
    {
        // Create a new instance with all required parameters
        return new StoryQueryParameters
        {
            Token = parameters.Token ?? _options.AccessToken ?? _options.OAuthToken,
            Version = parameters.Version,
            Language = parameters.Language,
            ResolveRelations = parameters.ResolveRelations,
            ResolveLinks = parameters.ResolveLinks,
            Cv = string.IsNullOrEmpty(parameters.Cv)
                ? GetCacheVersion().ToString(CultureInfo.InvariantCulture)
                : parameters.Cv,
            Page = parameters.Page,
            PerPage = parameters.PerPage,
            SearchTerm = parameters.SearchTerm,
            SortBy = parameters.SortBy,
            WithTag = parameters.WithTag,
            ExcludingFields = parameters.ExcludingFields,
            ByUuidsOrdered = parameters.ByUuidsOrdered,
            ByUuids = parameters.ByUuids,
            BySlugs = parameters.BySlugs,
            StartsWith = parameters.StartsWith,
            ResolveLevel = parameters.ResolveLevel,
            FallbackLang = parameters.FallbackLang
        };
    }
    private static Dictionary<string, string> BuildQueryParameters(StoryQueryParameters parameters, bool isMultipleStories)
    {
        var queryParams = new Dictionary<string, string>
        {
            ["token"] = parameters.Token ?? throw new InvalidOperationException("Access token is required")
        };

        if (!string.IsNullOrEmpty(parameters.Version))
            queryParams["version"] = parameters.Version;

        if (!string.IsNullOrEmpty(parameters.Language))
            queryParams["language"] = parameters.Language;

        // Only add pagination parameters for multiple stories
        if (isMultipleStories)
        {
            if (parameters.Page > 0)
                queryParams["page"] = parameters.Page.ToString(CultureInfo.InvariantCulture);

            if (parameters.PerPage > 0)
                queryParams["per_page"] = parameters.PerPage.ToString(CultureInfo.InvariantCulture);
        }

        if (!string.IsNullOrEmpty(parameters.Cv))
            queryParams["cv"] = parameters.Cv;

        if (!string.IsNullOrEmpty(parameters.StartsWith))
            queryParams["starts_with"] = parameters.StartsWith;

        if (!string.IsNullOrEmpty(parameters.ByUuids))
            queryParams["by_uuids"] = parameters.ByUuids;

        if (!string.IsNullOrEmpty(parameters.ByUuidsOrdered))
            queryParams["by_uuids_ordered"] = parameters.ByUuidsOrdered;

        if (!string.IsNullOrEmpty(parameters.FallbackLang))
            queryParams["fallback_lang"] = parameters.FallbackLang;

        if (!string.IsNullOrEmpty(parameters.ExcludingFields))
            queryParams["excluding_fields"] = parameters.ExcludingFields;

        if (!string.IsNullOrEmpty(parameters.ResolveLinks))
            queryParams["resolve_links"] = parameters.ResolveLinks;

        if (parameters.ResolveRelations?.Length > 0)
            queryParams["resolve_relations"] = string.Join(",", parameters.ResolveRelations);

        if (parameters.ResolveLevel.HasValue)
            queryParams["resolve_level"] = parameters.ResolveLevel.Value.ToString(CultureInfo.InvariantCulture);

        return queryParams;
    }

    private static string GenerateCacheKey(string endpoint, IDictionary<string, string> queryParams)
    {
        var orderedParams = string.Join("_", queryParams.OrderBy(x => x.Key).Select(x => $"{x.Key}={x.Value}"));
        return $"{endpoint}_{orderedParams}";
    }

    private async Task<T?> TryGetFromCache<T>(string key, CancellationToken cancellationToken) where T : class
    {
        try
        {
            return await _cache.GetAsync<T>(key, cancellationToken);
        }
        catch (Exception ex)
        {
            // Log cache error but continue
            System.Diagnostics.Debug.WriteLine($"Cache error: {ex.Message}");
            return null;
        }
    }

    private async Task CacheResponse<T>(string key, T response, string? token, CancellationToken cancellationToken) where T : class
    {
        try
        {
            await _cache.SetAsync(key, response, cancellationToken: cancellationToken);

            if (response is StoriesResponse<T> storiesResponse)
            {
                var cv = int.Parse(storiesResponse.Cv.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
                if (GetCacheVersion() != cv)
                {
                    await _cache.ClearAsync(cancellationToken);
                    SetCacheVersion(cv);
                }
            }
        }
        catch (Exception ex)
        {
            // Log cache error but continue
            System.Diagnostics.Debug.WriteLine($"Cache error: {ex.Message}");
        }
    }

    public int GetCacheVersion()
    {
        var token = _options.AccessToken ?? _options.OAuthToken;
        return token != null && _cacheVersions.TryGetValue(token, out var version)
            ? version
            : 0;
    }

    public void SetCacheVersion(int version)
    {
        var token = _options.AccessToken ?? _options.OAuthToken;
        if (token != null)
            _cacheVersions[token] = version;
    }

    public void ClearCacheVersion()
    {
        var token = _options.AccessToken ?? _options.OAuthToken;
        if (token != null)
            _cacheVersions[token] = 0;
    }

    public async Task ClearCacheAsync(CancellationToken cancellationToken = default)
    {
        await _cache.ClearAsync(cancellationToken);
        ClearCacheVersion();
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        try
        {
            if (_httpClient is IAsyncDisposable httpClientDisposable)
                await httpClientDisposable.DisposeAsync();
        }
        catch (Exception ex)
        {
            // Log the exception or handle it as needed
            throw new InvalidOperationException("An error occurred while disposing the HTTP client.", ex);
        }
        finally
        {
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}