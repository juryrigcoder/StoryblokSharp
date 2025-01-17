using Microsoft.Extensions.DependencyInjection;
using StoryblokSharp.Client;
using StoryblokSharp.Components;
using StoryblokSharp.Configuration;
using StoryblokSharp.Models.Cache;
using StoryblokSharp.Models.Common;
using StoryblokSharp.Models.RichText;
using StoryblokSharp.Services.RichText;
using StoryblokSharp.Services.RichText.NodeResolvers;
using StoryblokSharp.Utilities.RichText;
using Xunit;

namespace StoryblokSharpTests.Configuration;

public class StoryblokClientBuilderTests
{
    private readonly IServiceCollection _services;
    private readonly StoryblokClientBuilder _builder;

    public StoryblokClientBuilderTests()
    {
        _services = new ServiceCollection();
        _builder = new StoryblokClientBuilder(_services);
        _services.AddSingleton<IAttributeUtilities, AttributeUtilities>();
        _services.AddSingleton<MarkNodeResolver>(); 
        _services.AddSingleton<BlockNodeResolver>();
        _services.AddSingleton<ImageNodeResolver>();
        _services.AddSingleton<TextNodeResolver>();
    }

    [Fact]
    public void Constructor_WithNullServices_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new StoryblokClientBuilder(null!));
    }

    [Fact]
    public void WithAccessToken_ValidToken_SetsToken()
    {
        // Act
        var result = _builder.WithAccessToken("valid-token");

        // Assert
        Assert.Same(_builder, result);
        var client = BuildAndResolveClient();
        Assert.NotNull(client);
    }

    [Fact]
    public void WithAccessToken_NullToken_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _builder.WithAccessToken(null!));
    }

    [Fact]
    public void WithOAuthToken_ValidToken_SetsToken()
    {
        // Act
        var result = _builder.WithOAuthToken("valid-oauth-token");

        // Assert
        Assert.Same(_builder, result);
        var client = BuildAndResolveClient();
        Assert.NotNull(client);
    }

    [Fact]
    public void WithOAuthToken_NullToken_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _builder.WithOAuthToken(null!));
    }

    [Fact]
    public void WithRegion_ValidRegion_SetsRegion()
    {
        // Act
        var result = _builder.WithRegion(Region.US);

        // Assert
        Assert.Same(_builder, result);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void WithMaxRetries_NegativeValue_ThrowsArgumentOutOfRangeException(int maxRetries)
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => _builder.WithMaxRetries(maxRetries));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(10)]
    public void WithMaxRetries_ValidValue_SetsMaxRetries(int maxRetries)
    {
        // Act
        var result = _builder.WithMaxRetries(maxRetries);

        // Assert
        Assert.Same(_builder, result);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void WithTimeout_NegativeValue_ThrowsArgumentOutOfRangeException(int timeout)
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => _builder.WithTimeout(timeout));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(30)]
    [InlineData(60)]
    public void WithTimeout_ValidValue_SetsTimeout(int timeout)
    {
        // Act
        var result = _builder.WithTimeout(timeout);

        // Assert
        Assert.Same(_builder, result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void WithRateLimit_InvalidValue_ThrowsArgumentOutOfRangeException(int rateLimit)
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => _builder.WithRateLimit(rateLimit));
    }

    [Fact]
    public void WithHeaders_ValidHeaders_SetsHeaders()
    {
        // Arrange
        var headers = new Dictionary<string, string>
        {
            { "X-Custom-Header", "Value" }
        };

        // Act
        var result = _builder.WithHeaders(headers);

        // Assert
        Assert.Same(_builder, result);
    }

    [Fact]
    public void WithHeaders_NullHeaders_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _builder.WithHeaders(null!));
    }

    [Fact]
    public void WithCache_ValidConfiguration_ConfiguresCache()
    {
        // Act
        var result = _builder.WithCache(options =>
        {
            options.WithType(CacheType.Memory);
            options.WithDefaultExpiration(TimeSpan.FromMinutes(5));
        });

        // Assert
        Assert.Same(_builder, result);
    }

    [Fact]
    public void WithCache_NullConfiguration_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _builder.WithCache(null!));
    }

    [Fact]
    public void WithCustomCache_ValidProvider_SetsCustomCache()
    {
        // Arrange
        var customCache = new MockCacheProvider();

        // Act
        var result = _builder.WithCustomCache(customCache);

        // Assert
        Assert.Same(_builder, result);
    }

    [Fact]
    public void WithCustomCache_NullProvider_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _builder.WithCustomCache(null!));
    }

    [Fact]
    public void WithRichTextSchema_ValidSchema_SetsSchema()
    {
        // Arrange
        var schema = new MockRichTextSchema();

        // Act
        var result = _builder.WithRichTextSchema(schema);

        // Assert
        Assert.Same(_builder, result);
    }

    [Fact]
    public void WithRichTextSchema_NullSchema_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _builder.WithRichTextSchema(null!));
    }

    [Fact]
    public void WithHttpMessageHandler_ValidHandler_SetsHandler()
    {
        // Arrange
        var handler = new MockHttpMessageHandler();

        // Act
        var result = _builder.WithHttpMessageHandler(handler);

        // Assert
        Assert.Same(_builder, result);
    }

    [Fact]
    public void WithHttpMessageHandler_NullHandler_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _builder.WithHttpMessageHandler((HttpMessageHandler)null!));
    }

    [Fact]
    public void WithComponentResolver_ValidResolver_SetsResolver()
    {
        // Arrange
        Func<string, object, string> resolver = (type, data) => "";

        // Act
        var result = _builder.WithComponentResolver(resolver);

        // Assert
        Assert.Same(_builder, result);
    }

    [Fact]
    public void WithComponentResolver_NullResolver_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _builder.WithComponentResolver(null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void WithEndpoint_InvalidEndpoint_ThrowsArgumentException(string endpoint)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _builder.WithEndpoint(endpoint));
    }

    [Fact]
    public void WithEndpoint_ValidEndpoint_SetsEndpoint()
    {
        // Act
        var result = _builder.WithEndpoint("https://api.custom.com");

        // Assert
        Assert.Same(_builder, result);
    }

    [Fact]
    public void WithResponseInterceptor_ValidInterceptor_SetsInterceptor()
    {
        // Arrange
        Func<HttpResponseMessage, Task<HttpResponseMessage>> interceptor =
            response => Task.FromResult(response);

        // Act
        var result = _builder.WithResponseInterceptor(interceptor);

        // Assert
        Assert.Same(_builder, result);
    }

    [Fact]
    public void WithResponseInterceptor_NullInterceptor_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _builder.WithResponseInterceptor(null!));
    }

    [Fact]
    public void Build_WithoutTokens_ThrowsInvalidOperationException()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _builder.Build());
    }

    [Fact]
    public void Build_WithValidConfiguration_RegistersServices()
    {
        // Arrange
        _builder.WithAccessToken("valid-token")
               .WithCache(options => options
                   .WithType(CacheType.Memory))
               .WithMaxRetries(3)
               .WithTimeout(30);

        // Act
        var services = _builder.Build();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var client = serviceProvider.GetService<IStoryblokClient>();
        Assert.NotNull(client);
        Assert.IsType<StoryblokClient>(client);
    }

    private IStoryblokClient BuildAndResolveClient()
    {
        var services = _builder.Build();
        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<IStoryblokClient>();
    }

    // Mock classes for testing
    private class MockCacheProvider : ICacheProvider
    {
        public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class =>
            Task.FromResult<T?>(null);

        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class =>
            Task.CompletedTask;

        public Task<IDictionary<string, object>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IDictionary<string, object>>(new Dictionary<string, object>());

        public Task FlushAsync(CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private class MockRichTextSchema : IRichTextSchema
    {
        public IDictionary<string, Node> Nodes { get; } = new Dictionary<string, Node>();
        public IDictionary<string, MarkNode> Marks { get; } = new Dictionary<string, MarkNode>();

        Dictionary<string, Func<Node, SchemaResult>> IRichTextSchema.Nodes => throw new NotImplementedException();

        Dictionary<string, Func<Node, SchemaResult>> IRichTextSchema.Marks => throw new NotImplementedException();

        public string Render(Node node, IRichTextRenderer renderer) => "";
        public bool CanRender(Node node) => true;
    }

    private class MockHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage());
    }
}