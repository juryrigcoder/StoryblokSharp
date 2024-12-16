namespace StoryblokSharp.Models.RichText;

public record SchemaTag
{
    public required string Name { get; init; }
    public Dictionary<string, string>? Attributes { get; init; }
}


/// <summary>
/// Represents the schema result for a rich text node or mark
/// </summary>
public record SchemaNode
{
    /// <summary>
    /// The tag to use (e.g. "p", "div", "a href='...'")
    /// </summary>
    public string? Tag { get; init; }

    /// <summary>
    /// A self-closing tag to use (e.g. "hr", "br")
    /// </summary>
    public string? SingleTag { get; init; }

    /// <summary>
    /// Raw HTML to output
    /// </summary>
    public string? Html { get; init; }
}