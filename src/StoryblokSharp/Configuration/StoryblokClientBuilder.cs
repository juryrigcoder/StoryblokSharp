using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StoryblokSharp.Models.Common;
using StoryblokSharp.Models.Cache;
using StoryblokSharp.Models.Configuration;
using StoryblokSharp.Services.RichText;
using StoryblokSharp.Http;
using StoryblokSharp.Services.Cache;
using StoryblokSharp.Services.Throttling;
using StoryblokSharp.Client;
using StoryblokSharp.Utilities.RichText;

namespace StoryblokSharp.Configuration;

/// <summary>
/// Builder for configuring and creating a StoryblokClient
/// </summary>
public class StoryblokClientBuilder
{
    private string? _accessToken;
    private string? _oauthToken;
    private Region _region = Region.EU;
    private bool _useHttps = true;
    private int _maxRetries = 10;
    private int _timeout;
    private int _rateLimit = 5;
    private IDictionary<string, string>? _headers;
    private CacheOptions _cache = new();
    private ICacheProvider? _customCacheProvider;
    private IRichTextSchema? _customRichTextSchema;
    private Type? _customHttpHandlerType;
    private HttpMessageHandler? _customHttpHandler;
    private bool _resolveNestedRelations = true;
    private string? _endpoint;
    private Func<HttpResponseMessage, Task<HttpResponseMessage>>? _responseInterceptor;
    private Func<string, object, string>? _componentResolver;
    private RichTextOptions? _richTextOptions;

    private readonly IServiceCollection _services;

    /// <summary>
    /// Initializes a new instance of StoryblokClientBuilder
    /// </summary>
    public StoryblokClientBuilder(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        _services = services;
    }

    /// <summary>
    /// Sets the content delivery API access token
    /// </summary>
    public StoryblokClientBuilder WithAccessToken(string accessToken)
    {
        _accessToken = accessToken ?? throw new ArgumentNullException(nameof(accessToken));
        return this;
    }

    /// <summary>
    /// Sets the management API OAuth token
    /// </summary>
    public StoryblokClientBuilder WithOAuthToken(string oauthToken)
    {
        _oauthToken = oauthToken ?? throw new ArgumentNullException(nameof(oauthToken));
        return this;
    }

    /// <summary>
    /// Sets the API region
    /// </summary>
    public StoryblokClientBuilder WithRegion(Region region)
    {
        _region = region;
        return this;
    }

    /// <summary>
    /// Sets whether to use HTTPS (defaults to true)
    /// </summary>
    public StoryblokClientBuilder WithHttps(bool useHttps = true)
    {
        _useHttps = useHttps;
        return this;
    }

    /// <summary>
    /// Sets the maximum number of retry attempts
    /// </summary>
    public StoryblokClientBuilder WithMaxRetries(int maxRetries)
    {
        if (maxRetries < 0)
            throw new ArgumentOutOfRangeException(nameof(maxRetries), "Max retries must be non-negative");
        
        _maxRetries = maxRetries;
        return this;
    }

    /// <summary>
    /// Sets the request timeout in seconds
    /// </summary>
    public StoryblokClientBuilder WithTimeout(int timeoutSeconds)
    {
        if (timeoutSeconds < 0)
            throw new ArgumentOutOfRangeException(nameof(timeoutSeconds), "Timeout must be non-negative");
        
        _timeout = timeoutSeconds;
        return this;
    }

    /// <summary>
    /// Sets the rate limit in requests per second
    /// </summary>
    public StoryblokClientBuilder WithRateLimit(int requestsPerSecond)
    {
        if (requestsPerSecond <= 0)
            throw new ArgumentOutOfRangeException(nameof(requestsPerSecond), "Rate limit must be positive");
        
        _rateLimit = requestsPerSecond;
        return this;
    }

    /// <summary>
    /// Sets custom headers to include with requests
    /// </summary>
    public StoryblokClientBuilder WithHeaders(IDictionary<string, string> headers)
    {
        _headers = headers ?? throw new ArgumentNullException(nameof(headers));
        return this;
    }

    /// <summary>
    /// Configures the caching behavior
    /// </summary>
    public StoryblokClientBuilder WithCache(Action<CacheOptionsBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
            
        var builder = new CacheOptionsBuilder();
        configure(builder);
        _cache = builder.Build();
        return this;
    }

    /// <summary>
    /// Sets a custom cache provider
    /// </summary>
    public StoryblokClientBuilder WithCustomCache(ICacheProvider cacheProvider)
    {
        _customCacheProvider = cacheProvider ?? throw new ArgumentNullException(nameof(cacheProvider));
        _cache = new CacheOptions 
        { 
            Type = CacheType.Custom, 
            Custom = cacheProvider 
        };
        return this;
    }

    /// <summary>
    /// Sets a custom rich text rendering schema
    /// </summary>
    public StoryblokClientBuilder WithRichTextSchema(IRichTextSchema schema)
    {
        _customRichTextSchema = schema ?? throw new ArgumentNullException(nameof(schema));
        return this;
    }

    /// <summary>
    /// Sets a custom HTTP message handler type
    /// </summary>
    public StoryblokClientBuilder WithHttpMessageHandler<THandler>() where THandler : DelegatingHandler
    {
        _customHttpHandlerType = typeof(THandler);
        return this;
    }

    /// <summary>
    /// Sets a custom HTTP message handler instance
    /// </summary>
    public StoryblokClientBuilder WithHttpMessageHandler(HttpMessageHandler handler)
    {
        _customHttpHandler = handler ?? throw new ArgumentNullException(nameof(handler));
        return this;
    }

    /// <summary>
    /// Sets a custom component resolver
    /// </summary>
    public StoryblokClientBuilder WithComponentResolver(Func<string, object, string> resolver)
    {
        _componentResolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        return this;
    }

    /// <summary>
    /// Sets a custom API endpoint
    /// </summary>
    public StoryblokClientBuilder WithEndpoint(string endpoint)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
            throw new ArgumentException("Endpoint cannot be null or empty", nameof(endpoint));
        
        _endpoint = endpoint;
        return this;
    }

    /// <summary>
    /// Sets a custom response interceptor
    /// </summary>
    public StoryblokClientBuilder WithResponseInterceptor(
        Func<HttpResponseMessage, Task<HttpResponseMessage>> interceptor)
    {
        _responseInterceptor = interceptor ?? throw new ArgumentNullException(nameof(interceptor));
        return this;
    }

    /// <summary>
    /// Sets the rich text rendering options
    /// </summary>
    public StoryblokClientBuilder WithRichTextOptions(RichTextOptions options)
    {
        _richTextOptions = options ?? throw new ArgumentNullException(nameof(options));
        return this;
    }

    /// <summary>
    /// Completes the configuration and registers services
    /// </summary>
    public IServiceCollection Build()
    {
        if (string.IsNullOrEmpty(_accessToken) && string.IsNullOrEmpty(_oauthToken))
        {
            throw new InvalidOperationException(
                "Either an access token or OAuth token must be provided");
        }

        // Create options instance
        var options = new StoryblokOptions
        {
            AccessToken = _accessToken,
            OAuthToken = _oauthToken,
            Region = (Models.Region)_region,
            UseHttps = _useHttps,
            MaxRetries = _maxRetries,
            Timeout = _timeout,
            RateLimit = _rateLimit,
            Headers = _headers,
            Cache = _cache,
            ComponentResolver = _componentResolver,
            ResolveNestedRelations = _resolveNestedRelations,
            Endpoint = _endpoint,
            ResponseInterceptor = _responseInterceptor,
            RichTextSchema = _customRichTextSchema
        };

        // Register options
        _services.AddSingleton(Options.Create(options));

        // Register HTTP client
        if (_customHttpHandlerType != null)
        {
            _services.AddTransient(_customHttpHandlerType);
            _services.AddHttpClient<IStoryblokHttpClient, StoryblokHttpClient>()
                .AddHttpMessageHandler(sp => (DelegatingHandler)sp.GetRequiredService(_customHttpHandlerType));
        }
        else if (_customHttpHandler != null)
        {
            _services.AddHttpClient<IStoryblokHttpClient, StoryblokHttpClient>()
                .ConfigurePrimaryHttpMessageHandler(() => _customHttpHandler);
        }
        else
        {
            _services.AddHttpClient<IStoryblokHttpClient, StoryblokHttpClient>();
        }

        // Register cache services
        if (_cache.Type == CacheType.Memory)
        {
            _services.AddMemoryCache();
            _services.AddSingleton<IStoryblokCache, MemoryStoryblokCache>();
        }
        else if (_cache.Type == CacheType.Custom && _customCacheProvider != null)
        {
            _services.AddSingleton(_customCacheProvider);
            _services.AddSingleton<IStoryblokCache, MemoryStoryblokCache>();
        }
        else
        {
            _services.AddSingleton<IStoryblokCache, NullStoryblokCache>();
        }

        // Register rich text services
        if (_richTextOptions != null)
        {
            _services.AddSingleton(Options.Create(_richTextOptions));
        }

        _services.AddSingleton<IHtmlUtilities, HtmlUtilities>();
        _services.AddSingleton<StringBuilderCache>();
        _services.AddSingleton<AttributeUtilities>();
        _services.AddSingleton<IHtmlSanitizer, HtmlSanitizer>();
        _services.AddScoped<IRichTextRenderer, RichTextRenderer>();

        if (_customRichTextSchema != null)
        {
            _services.AddSingleton(_customRichTextSchema);
        }
        _services.AddSingleton<IRichTextSchema>(sp => 
            _customRichTextSchema ?? new DefaultRichTextSchema());

        // Register throttling service
        _services.AddSingleton<IThrottleService>(_ => new ThrottleService(_rateLimit));

        // Register main client
        _services.AddScoped<IStoryblokClient, StoryblokClient>();

        return _services;
    }
}