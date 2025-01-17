using System.Diagnostics;
using StoryblokSharp.Services.Throttling;
using Xunit;

namespace StoryblokSharpTests.Services;

public class ThrottleServiceTests
{
    [Fact]
    public void Constructor_WithValidInitialRate_CreatesInstance()
    {
        // Arrange & Act
        var service = new ThrottleService(5);

        // Assert
        Assert.NotNull(service);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void SetRateLimit_WithInvalidRate_ThrowsArgumentOutOfRangeException(int invalidRate)
    {
        // Arrange
        var service = new ThrottleService();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => service.SetRateLimit(invalidRate));
    }

    [Fact]
    public void SetRateLimit_WithValidRate_DoesNotThrow()
    {
        // Arrange
        var service = new ThrottleService();

        // Act & Assert
        var exception = Record.Exception(() => service.SetRateLimit(10));
        Assert.Null(exception);
    }

    [Fact]
    public async Task ExecuteAsync_RespectsRateLimit()
    {
        // Arrange
        const int requestsPerSecond = 5;
        const int totalRequests = 10;
        var service = new ThrottleService(requestsPerSecond);
        var stopwatch = new Stopwatch();
        var results = new List<int>();

        // Act
        stopwatch.Start();
        var tasks = new List<Task>();
        for (int i = 0; i < totalRequests; i++)
        {
            var index = i;
            tasks.Add(service.ExecuteAsync(async ct =>
            {
                results.Add(index);
                return index;
            }));
        }
        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        // Should take at least 1 second for 10 requests at 5 RPS
        Assert.True(stopwatch.ElapsedMilliseconds >= 1000);
        Assert.Equal(totalRequests, results.Count);
    }

    [Fact]
    public async Task ExecuteAsync_WithCancellation_CancelsOperation()
    {
        // Arrange
        var service = new ThrottleService(1);
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(100); // Cancel after 100ms

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.ExecuteAsync<int>(async ct =>
            {
                await Task.Delay(1000, ct);
                return 1;
            }, cts.Token));

        Assert.True(exception is TaskCanceledException || exception is OperationCanceledException);
    }

    [Fact]
    public async Task ExecuteAsync_WhenActionThrows_PropagatesException()
    {
        // Arrange
        var service = new ThrottleService();
        var expectedException = new InvalidOperationException("Test exception");

        // Act & Assert
        var actualException = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ExecuteAsync<int>(ct =>
                Task.FromException<int>(expectedException)));

        Assert.Same(expectedException, actualException);
    }

    [Fact]
    public async Task ExecuteAsync_AfterRateLimitChange_RespectsNewLimit()
    {
        // Arrange
        const int initialRateLimit = 5;
        const int newRateLimit = 2;
        const int totalRequests = 6;
        var service = new ThrottleService(initialRateLimit);
        var stopwatch = new Stopwatch();
        var results = new List<int>();

        // Act
        service.SetRateLimit(newRateLimit);
        stopwatch.Start();
        var tasks = new List<Task>();
        for (int i = 0; i < totalRequests; i++)
        {
            var index = i;
            tasks.Add(service.ExecuteAsync(async ct =>
            {
                results.Add(index);
                return index;
            }));
        }
        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        // Should take at least 2 seconds for 6 requests at 2 RPS
        Assert.True(stopwatch.ElapsedMilliseconds >= 2000);
        Assert.Equal(totalRequests, results.Count);
    }
}