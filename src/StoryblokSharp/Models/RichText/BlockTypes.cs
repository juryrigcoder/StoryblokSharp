namespace StoryblokSharp.Models.RichText;

/// <summary>
/// Defines the block-level element types in a rich text document
/// </summary>
public enum BlockTypes
{
    /// <summary>Root document node</summary>
    Document,

    /// <summary>Header element with level attribute</summary>
    Heading,

    /// <summary>Paragraph element</summary>
    Paragraph,
    
    /// <summary>Blockquote element</summary>
    Quote,
    
    /// <summary>Ordered list element</summary>
    OrderedList,
    
    /// <summary>Unordered list element</summary>
    BulletList,
    
    /// <summary>List item element</summary>
    ListItem,
    
    /// <summary>Code block element</summary>
    CodeBlock,
    
    /// <summary>Horizontal rule element</summary>
    HorizontalRule,
    
    /// <summary>Line break element</summary>
    HardBreak,
    
    /// <summary>Image element</summary>
    Image,
    
    /// <summary>Emoji element</summary>
    Emoji,
    
    /// <summary>Custom Storyblok component</summary>
    Component
}