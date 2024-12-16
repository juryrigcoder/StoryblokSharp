namespace StoryblokSharp.Exceptions;

/// <summary>
/// Exception thrown when a Storyblok API request fails
/// </summary>
public class StoryblokApiException : Exception
{
    /// <summary>
    /// The HTTP status code of the response
    /// </summary>
    public int? StatusCode { get; }

    /// <summary>
    /// The raw response content
    /// </summary>
    public string? Content { get; init; }

    public StoryblokApiException(string message) : base(message)
    {
    }

    public StoryblokApiException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }

    public StoryblokApiException(HttpResponseMessage response) 
        : base($"API request failed with status code {(int)response.StatusCode}")
    {
        StatusCode = (int)response.StatusCode;
    }
}