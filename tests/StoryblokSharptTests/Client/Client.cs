using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using StoryblokSharp.Client;
using StoryblokSharp.Models.Configuration;
using StoryblokSharp.Models.Stories;
using StoryblokSharp.Services.Cache;
using StoryblokSharp.Services.Throttling;
using StoryblokSharp.Http;

namespace StoryblokSharp.Tests;

public class StoryblokClientTests
{
    private readonly Mock<IStoryblokHttpClient> _mockHttpClient;
    private readonly Mock<IStoryblokCache> _mockCache;
    private readonly Mock<IThrottleService> _mockThrottle;
    private readonly Mock<IRichTextRenderer> _mockRichTextRenderer;
    private readonly StoryblokOptions _options;
    private readonly StoryblokClient _client;

    public StoryblokClientTests()
    {
        _mockHttpClient = new Mock<IStoryblokHttpClient>();
        _mockCache = new Mock<IStoryblokCache>();
        _mockThrottle = new Mock<IThrottleService>();
        _mockRichTextRenderer = new Mock<IRichTextRenderer>();
        
        _options = new StoryblokOptions
        {
            AccessToken = "test-token",
            RateLimit = 5
        };

        _client = new StoryblokClient(
            _mockHttpClient.Object,
            _mockCache.Object,
            _mockThrottle.Object,
            _mockRichTextRenderer.Object,
            Options.Create(_options)
        );
    }

    [Fact]
    public async Task GetStoryAsync_ValidSlug_ReturnsStory()
    {
        // Arrange
        var slug = "test-story";
        var expectedResponse = new StoryResponse<TestContent>
        {
            Story = new Story<TestContent>
            {
                Id = 1,
                Uuid = "test-uuid",
                Name = "Test Story",
                Slug = slug,
                FullSlug = slug,
                Content = new TestContent(),
                CreatedAt = DateTime.UtcNow
            },
            Cv = 123
        };

        _mockThrottle
            .Setup(x => x.ExecuteAsync(It.IsAny<Func<CancellationToken, Task<StoryResponse<TestContent>>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _client.GetStoryAsync<TestContent>(slug);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponse.Story.Id, result.Story.Id);
        Assert.Equal(expectedResponse.Story.Name, result.Story.Name);
        Assert.Equal(expectedResponse.Story.Slug, result.Story.Slug);
    }

    [Fact]
    public async Task GetStoryAsync_CachedStory_ReturnsCachedResult()
    {
        // Arrange
        var slug = "test-story";
        var cachedResponse = new StoryResponse<TestContent>
        {
            Story = new Story<TestContent>
            {
                Id = 1,
                Uuid = "test-uuid",
                Name = "Cached Story",
                Slug = slug,
                FullSlug = slug,
                Content = new TestContent(),
                CreatedAt = DateTime.UtcNow
            },
            Cv = 123
        };

        _mockCache
            .Setup(x => x.GetAsync<StoryResponse<TestContent>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedResponse);

        // Act
        var result = await _client.GetStoryAsync<TestContent>(slug, new StoryQueryParameters { Version = "published" });

        // Assert
        Assert.NotNull(result);
        Assert.Equal(cachedResponse.Story.Id, result.Story.Id);
        Assert.Equal(cachedResponse.Story.Name, result.Story.Name);
        
        // Verify HTTP client was not called
        _mockHttpClient.Verify(
            x => x.GetAsync<StoryResponse<TestContent>>(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()), 
            Times.Never);
    }

    [Fact]
    public async Task GetStoryAsync_DraftVersion_SkipsCache()
    {
        // Arrange
        var slug = "test-story";
        var expectedResponse = new StoryResponse<TestContent>
        {
            Story = new Story<TestContent>
            {
                Id = 1,
                Uuid = "test-uuid",
                Name = "Test Story",
                Slug = slug,
                FullSlug = slug,
                Content = new TestContent(),
                CreatedAt = DateTime.UtcNow
            },
            Cv = 123
        };

        _mockThrottle
            .Setup(x => x.ExecuteAsync(It.IsAny<Func<CancellationToken, Task<StoryResponse<TestContent>>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _client.GetStoryAsync<TestContent>(slug, new StoryQueryParameters { Version = "draft" });

        // Assert
        Assert.NotNull(result);
        
        // Verify cache was not checked
        _mockCache.Verify(
            x => x.GetAsync<StoryResponse<TestContent>>(It.IsAny<string>(), It.IsAny<CancellationToken>()), 
            Times.Never);
    }

[Theory]
[InlineData(null)]
[InlineData("")]
[InlineData(" ")]
[InlineData("   ")]
public async Task GetStoryAsync_InvalidSlug_ThrowsArgumentNullException(string? slug)
{
    // Act & Assert
    var exception = await Assert.ThrowsAsync<ArgumentNullException>(
        async () => await _client.GetStoryAsync<TestContent>(slug!));
    
    Assert.Equal("slug", exception.ParamName);
    Assert.Contains("Story slug cannot be null or whitespace", exception.Message);
}

    [Fact]
    public async Task GetStoriesAsync_ValidParameters_ReturnsStories()
    {
        // Arrange
        var expectedResponse = new StoriesResponse<TestContent>
        {
            Stories = new[]
            {
                new Story<TestContent>
                {
                    Id = 1,
                    Uuid = "test-uuid-1",
                    Name = "Test Story 1",
                    Slug = "test-1",
                    FullSlug = "test-1",
                    Content = new TestContent(),
                    CreatedAt = DateTime.UtcNow
                },
                new Story<TestContent>
                {
                    Id = 2,
                    Uuid = "test-uuid-2", 
                    Name = "Test Story 2",
                    Slug = "test-2",
                    FullSlug = "test-2",
                    Content = new TestContent(),
                    CreatedAt = DateTime.UtcNow
                }
            },
            Cv = 123,
            Rels = new Story<TestContent>[] { },
            Links = Array.Empty<Link>()
        };

        _mockThrottle
            .Setup(x => x.ExecuteAsync(It.IsAny<Func<CancellationToken, Task<StoriesResponse<TestContent>>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _client.GetStoriesAsync<TestContent>(new StoryQueryParameters());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponse.Stories.Length, result.Stories.Length);
    }

    [Fact]
    public void SetCacheVersion_ValidVersion_UpdatesVersion()
    {
        // Arrange
        var version = 123;

        // Act
        _client.SetCacheVersion(version);

        // Assert
        Assert.Equal(version, _client.GetCacheVersion());
    }

    [Fact]
    public void ClearCacheVersion_ResetsVersion()
    {
        // Arrange
        _client.SetCacheVersion(123);

        // Act
        _client.ClearCacheVersion();

        // Assert
        Assert.Equal(0, _client.GetCacheVersion());
    }
}

// Test content class for story responses
public class TestContent
{
    public string? Title { get; set; }
    public string? Description { get; set; }
}