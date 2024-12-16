namespace StoryblokSharp.Models.RichText;

/// <summary>
/// Base interface for rich text nodes
/// </summary>
public interface IRichTextNode
{
    /// <summary>Gets the node type</summary>
    string Type { get; }
    
    /// <summary>Gets the node attributes</summary>
    IDictionary<string, object>? Attrs { get; }
    
    /// <summary>Gets the node content</summary>
    IEnumerable<IRichTextNode>? Content { get; }
    
    /// <summary>Gets the text content</summary>
    string? Text { get; }
}

/// <summary>
/// Base record for rich text nodes
/// </summary>
public record RichTextNode : IRichTextNode
{
    /// <inheritdoc/>
    public required string Type { get; init; }
    
    /// <inheritdoc/>
    public IDictionary<string, object>? Attrs { get; init; }
    
    /// <inheritdoc/>
    public IEnumerable<IRichTextNode>? Content { get; init; }
    
    /// <inheritdoc/>
    public string? Text { get; init; }
    
    /// <summary>Gets the mark nodes</summary>
    public IEnumerable<MarkNode>? Marks { get; init; }
}

/// <summary>
/// Represents a mark node that can be applied to text
/// </summary>
public record MarkNode : RichTextNode
{
    /// <summary>Gets the mark type</summary>
    public required MarkTypes MarkType { get; init; }
    
    /// <summary>Gets the link type if this is a link mark</summary> 
    public LinkTypes? LinkType { get; init; }
}

/// <summary>
/// Represents a text node with marks
/// </summary>
public record TextNode : RichTextNode
{
    /// <summary>Gets the text content</summary>
    public required string TextContent { get; init; }
}

/// <summary>
/// Link types supported in rich text
/// </summary>
public enum LinkTypes
{
    /// <summary>External URL</summary>
    Url,
    
    /// <summary>Internal story link</summary>
    Story,
    
    /// <summary>Asset link</summary>
    Asset,
    
    /// <summary>Email link</summary>
    Email
}
