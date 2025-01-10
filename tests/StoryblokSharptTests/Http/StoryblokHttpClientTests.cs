using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using StoryblokSharp.Exceptions;
using StoryblokSharp.Http;
using StoryblokSharp.Models.Configuration;
using Xunit;

namespace StoryblokSharp.Tests.Http;

public class StoryblokHttpClientTests
{
    private readonly Mock<ILogger> _mockLogger;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly StoryblokOptions _options;
    private readonly HttpClient _httpClient;
    private readonly StoryblokHttpClient _client;

    public StoryblokHttpClientTests()
    {
        _mockLogger = new Mock<ILogger>();
        _mockLogger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _options = new StoryblokOptions
        {
            AccessToken = "test-token",
            MaxRetries = 3
        };

        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        _client = new StoryblokHttpClient(
            _httpClient,
            Options.Create(_options),
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task GetAsync_SuccessfulRequest_LogsAppropriateMessages()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"story\": {\"id\": 1, \"name\": \"Test Story\"}}")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act
        await _client.GetAsync<object>("test-endpoint");

        // Assert
        VerifyLog(LogLevel.Information, "Making GET request", Times.Once());
        VerifyLog(LogLevel.Debug, "Attempting request", Times.Once());
        VerifyLog(LogLevel.Debug, "Response Status", Times.Once());
    }

    [Fact]
    public async Task GetAsync_RateLimitResponse_LogsRetryAttempts()
    {
        // Arrange
        var rateLimitResponse = new HttpResponseMessage(HttpStatusCode.TooManyRequests)
        {
            Headers = { RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(TimeSpan.FromSeconds(1)) }
        };
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"story\": {\"id\": 1, \"name\": \"Test Story\"}}")
        };

        _mockHttpMessageHandler
            .Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(rateLimitResponse)
            .ReturnsAsync(successResponse);

        // Act
        await _client.GetAsync<object>("test-endpoint");

        // Assert
        VerifyLog(LogLevel.Information, "Rate limited", Times.Once());
        VerifyLog(LogLevel.Debug, "Attempting request", Times.Exactly(2)); // Initial request + 1 retry
    }

    [Fact]
    public async Task GetAsync_MaxRetriesExceeded_LogsMaxRetriesMessage()
    {
        // Arrange
        var rateLimitResponse = new HttpResponseMessage(HttpStatusCode.TooManyRequests)
        {
            Headers = { RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(TimeSpan.FromSeconds(1)) }
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(rateLimitResponse);

        // Act & Assert
        await Assert.ThrowsAsync<StoryblokApiException>(
            async () => await _client.GetAsync<object>("test-endpoint"));

        VerifyLog(LogLevel.Warning, "Max retries reached", Times.Once());
    }

    [Fact]
    public async Task GetAsync_DeserializationError_LogsError()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("invalid json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act & Assert
        await Assert.ThrowsAsync<StoryblokApiException>(
            async () => await _client.GetAsync<object>("test-endpoint"));

        VerifyLog(LogLevel.Error, "JSON Deserialization error", Times.Once());
    }

    [Fact]
    public async Task GetAsync_RequestCancelled_LogsCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Returns<HttpRequestMessage, CancellationToken>(async (_, token) =>
            {
                cts.Cancel();
                throw new TaskCanceledException();
            });

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(
            async () => await _client.GetAsync<object>("test-endpoint", cancellationToken: cts.Token));

        VerifyLog(LogLevel.Information, "Request cancelled", Times.Once());
    }

    [Fact]
    public async Task SetLogger_UpdatesLogger()
    {
        // Arrange
        var newLogger = new Mock<ILogger>();
        newLogger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        // Act
        _client.SetLogger(newLogger.Object);

        // Assert - Make a request to verify the new logger is used
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"test\": \"value\"}")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        await _client.GetAsync<object>("test-endpoint");

        // The original mock logger should not receive any new logs
        VerifyNoLogMessages(_mockLogger);
    }

    private void VerifyLog(LogLevel level, string messageContains, Times times)
    {
        _mockLogger.Verify(
            x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(messageContains)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times);
    }

    private void VerifyNoLogMessages(Mock<ILogger> logger)
    {
        logger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }
}