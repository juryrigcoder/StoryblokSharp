using StoryblokSharp.Models.RichText;

namespace StoryblokSharp.Services.RichText;

/// <summary>
/// Schema result object
/// </summary>
public class SchemaResult
{
    public string[]? Tag { get; set; }
    public string? SingleTag { get; set; }
    public Dictionary<string, object>? Attrs { get; set; }
    public string? Html { get; set; }
}

/// <summary>
/// Interface for rich text schema definition
/// </summary>
public interface IRichTextSchema
{
    /// <summary>
    /// Gets the node schemas
    /// </summary>
    Dictionary<string, Func<Node, SchemaResult>> Nodes { get; }

    /// <summary>
    /// Gets the mark schemas
    /// </summary>
    Dictionary<string, Func<Node, SchemaResult>> Marks { get; }
}

/// <summary>
/// Base class for rich text schema implementation
/// </summary>
public abstract class RichTextSchema : IRichTextSchema
{
    /// <summary>
    /// Gets or sets the node schemas
    /// </summary>
    public Dictionary<string, Func<Node, SchemaResult>> Nodes { get; protected set; } = new();

    /// <summary>
    /// Gets or sets the mark schemas
    /// </summary>
    public Dictionary<string, Func<Node, SchemaResult>> Marks { get; protected set; } = new();
}