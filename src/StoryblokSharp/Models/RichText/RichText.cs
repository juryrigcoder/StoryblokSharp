namespace StoryblokSharp.Models.RichText;

/// <summary>
/// Represents rich text content structure
/// </summary>
public class RichTextContent
{
    public required string Type { get; init; }
    public RichTextContent[]? Content { get; init; }
    public Mark[]? Marks { get; init; }
    public string? Text { get; init; }
    public Dictionary<string, object>? Attrs { get; init; }
}

/// <summary>
/// Represents a mark in rich text
/// </summary>
public class Mark
{
    public required string Type { get; init; }
    public Dictionary<string, object>? Attrs { get; init; }
}

/// <summary>
/// Represents a node in rich text
/// </summary>
public class Node
{
    public required string Type { get; init; }
    public Dictionary<string, object>? Attrs { get; init; }
    public string[]? Content { get; internal set; }
}

/// <summary>
/// Options for rendering rich text
/// </summary>
public class RenderOptions
{
    public bool OptimizeImages { get; init; }
    public ImageOptions? ImageOptions { get; init; }
}

/// <summary>
/// Options for image optimization
/// </summary>
public class ImageOptions
{
    public int? Width { get; set; }
    public int? Height { get; set; }
    public string? Loading { get; set; }
    public string? Class { get; set; }
    public ImageFilters? Filters { get; set; }
}

/// <summary>
/// Image filter options
/// </summary>
public class ImageFilters
{
    public int? Blur { get; set; }
    public int? Brightness { get; set; }
    public string? Fill { get; set; }
    public string? Format { get; set; }
    public bool? Grayscale { get; set; }
    public int? Quality { get; set; }
    public int? Rotate { get; set; }
}