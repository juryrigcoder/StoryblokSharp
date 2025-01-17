using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using StoryblokSharp.Models.Configuration;
using StoryblokSharp.Services.Cache;
using Xunit;

namespace StoryblokSharpTests.Services.Cache;

public class MemoryStoryblokCacheTests : IDisposable
{
    private readonly IMemoryCache _memoryCache;
    private readonly StoryblokOptions _options;
    private readonly MemoryStoryblokCache _cache;

    public MemoryStoryblokCacheTests()
    {
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _options = new StoryblokOptions
        {
            Cache = new StoryblokSharp.Models.Cache.CacheOptions
            {
                DefaultExpiration = TimeSpan.FromMinutes(5)
            }
        };
        var optionsWrapper = Options.Create(_options);
        _cache = new MemoryStoryblokCache(_memoryCache, optionsWrapper);
    }

    [Fact]
    public void Constructor_WithNullCache_ThrowsArgumentNullException()
    {
        // Arrange
        var options = Options.Create(_options);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new MemoryStoryblokCache(null!, options));
    }

    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        // Arrange
        var options = Options.Create(new StoryblokOptions { Cache = null });

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new MemoryStoryblokCache(_memoryCache, options));
    }

    [Fact]
    public async Task GetAsync_WhenKeyExists_ReturnsValue()
    {
        // Arrange
        var key = "test-key";
        var expectedValue = new TestCacheItem { Id = 1, Name = "Test" };
        await _cache.SetAsync(key, expectedValue);

        // Act
        var result = await _cache.GetAsync<TestCacheItem>(key);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedValue.Id, result.Id);
        Assert.Equal(expectedValue.Name, result.Name);
    }

    [Fact]
    public async Task GetAsync_WhenKeyDoesNotExist_ReturnsNull()
    {
        // Arrange
        var key = "non-existent-key";

        // Act
        var result = await _cache.GetAsync<TestCacheItem>(key);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SetAsync_WithCustomExpiration_RespondsToExpiration()
    {
        // Arrange
        var key = "expiring-key";
        var value = new TestCacheItem { Id = 1, Name = "Test" };
        var shortExpiration = TimeSpan.FromMilliseconds(100);

        // Act
        await _cache.SetAsync(key, value, shortExpiration);
        var beforeExpirationResult = await _cache.GetAsync<TestCacheItem>(key);
        await Task.Delay(200); // Wait for expiration
        var afterExpirationResult = await _cache.GetAsync<TestCacheItem>(key);

        // Assert
        Assert.NotNull(beforeExpirationResult);
        Assert.Null(afterExpirationResult);
    }

    [Fact]
    public async Task SetAsync_WithDefaultExpiration_UsesConfiguredExpiration()
    {
        // Arrange
        var key = "default-expiration-key";
        var value = new TestCacheItem { Id = 1, Name = "Test" };

        // Act
        await _cache.SetAsync(key, value);
        var result = await _cache.GetAsync<TestCacheItem>(key);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task RemoveAsync_WhenKeyExists_RemovesValue()
    {
        // Arrange
        var key = "remove-key";
        var value = new TestCacheItem { Id = 1, Name = "Test" };
        await _cache.SetAsync(key, value);

        // Act
        await _cache.RemoveAsync(key);
        var result = await _cache.GetAsync<TestCacheItem>(key);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task RemoveAsync_WhenKeyDoesNotExist_DoesNotThrow()
    {
        // Arrange
        var key = "non-existent-key";

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => _cache.RemoveAsync(key));
        Assert.Null(exception);
    }

    [Fact]
    public async Task ClearAsync_RemovesAllItems()
    {
        // Arrange
        var key1 = "key1";
        var key2 = "key2";
        var value = new TestCacheItem { Id = 1, Name = "Test" };
        await _cache.SetAsync(key1, value);
        await _cache.SetAsync(key2, value);

        // Act
        await _cache.ClearAsync();
        var result1 = await _cache.GetAsync<TestCacheItem>(key1);
        var result2 = await _cache.GetAsync<TestCacheItem>(key2);

        // Assert
        Assert.Null(result1);
        Assert.Null(result2);
    }

    [Fact]
    public async Task Operations_WithCancellation_HandlesCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => 
            _cache.ClearAsync(cts.Token));
    }

    public void Dispose()
    {
        _memoryCache.Dispose();
        _cache.Dispose();
    }

    private class TestCacheItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}

public class NullStoryblokCacheTests
{
    private readonly NullStoryblokCache _cache = new();

    [Fact]
    public async Task GetAsync_AlwaysReturnsNull()
    {
        // Act
        var result = await _cache.GetAsync<TestCacheItem>("any-key");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SetAsync_DoesNotThrow()
    {
        // Arrange
        var value = new TestCacheItem { Id = 1, Name = "Test" };

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => 
            _cache.SetAsync("any-key", value));
        Assert.Null(exception);
    }

    [Fact]
    public async Task RemoveAsync_DoesNotThrow()
    {
        // Act & Assert
        var exception = await Record.ExceptionAsync(() => 
            _cache.RemoveAsync("any-key"));
        Assert.Null(exception);
    }

    [Fact]
    public async Task ClearAsync_DoesNotThrow()
    {
        // Act & Assert
        var exception = await Record.ExceptionAsync(() => 
            _cache.ClearAsync());
        Assert.Null(exception);
    }

    private class TestCacheItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}