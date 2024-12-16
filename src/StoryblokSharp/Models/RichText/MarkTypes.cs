namespace StoryblokSharp.Models.RichText;

/// <summary>
/// Defines the inline mark types that can be applied to text nodes
/// </summary>
public enum MarkTypes
{
    /// <summary>Bold text</summary>
    Bold,

    /// <summary>Strong emphasis</summary>
    Strong,

    /// <summary>Strikethrough text</summary>
    Strike,

    /// <summary>Underlined text</summary>
    Underline,

    /// <summary>Italic text</summary>
    Italic,

    /// <summary>Code text</summary>
    Code,

    /// <summary>External or internal link</summary>
    Link,

    /// <summary>Internal anchor link</summary>
    Anchor,

    /// <summary>Custom styled text</summary>
    Styled,

    /// <summary>Superscript text</summary>
    Superscript,

    /// <summary>Subscript text</summary>
    Subscript,

    /// <summary>Text with custom styling</summary>
    TextStyle,

    /// <summary>Highlighted text</summary>
    Highlight
}
