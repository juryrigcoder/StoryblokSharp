namespace StoryblokSharp.Models.RichText;

/// <summary>
/// Strategy for handling invalid nodes in rich text content
/// </summary>
public enum InvalidNodeStrategy
{
    /// <summary>
    /// Remove invalid nodes from output
    /// </summary>
    Remove = 0,

    /// <summary>
    /// Replace invalid nodes with placeholder
    /// </summary>
    Replace = 1,

    /// <summary>
    /// Keep invalid nodes as-is in output
    /// </summary>
    Keep = 2,

    /// <summary>
    /// Throw exception when encountering invalid nodes
    /// </summary>
    Throw = 3
}

/// <summary>
/// Extension methods for InvalidNodeStrategy
/// </summary>
public static class InvalidNodeStrategyExtensions
{
    /// <summary>
    /// Handles content according to the specified strategy
    /// </summary>
    /// <param name="strategy">The strategy to apply</param>
    /// <param name="nodeType">The type of node being handled</param>
    /// <param name="content">The node's content</param>
    /// <param name="error">Optional error information</param>
    /// <returns>The processed content string</returns>
    public static string HandleContent(
        this InvalidNodeStrategy strategy,
        string nodeType,
        string? content = null,
        Exception? error = null)
    {
        return strategy switch
        {
            InvalidNodeStrategy.Remove => string.Empty,
            InvalidNodeStrategy.Replace => CreatePlaceholder(nodeType, error),
            InvalidNodeStrategy.Keep => content ?? string.Empty,
            InvalidNodeStrategy.Throw => throw new InvalidOperationException(
                $"Invalid node type: {nodeType}", 
                error),
            _ => string.Empty
        };
    }

    /// <summary>
    /// Creates a placeholder comment for replaced content
    /// </summary>
    private static string CreatePlaceholder(string nodeType, Exception? error = null)
    {
        if (error != null)
        {
            return $"<!-- Invalid node of type '{nodeType}': {error.Message} -->";
        }
        return $"<!-- Invalid node of type '{nodeType}' -->";
    }
}