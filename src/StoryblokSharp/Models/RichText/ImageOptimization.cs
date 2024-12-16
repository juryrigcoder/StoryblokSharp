namespace StoryblokSharp.Models.RichText;

/// <summary>
/// Options for image optimization
/// </summary>
public record ImageOptimizationOptions
{
    /// <summary>Gets or sets the image width</summary>
    public int? Width { get; init; }

    /// <summary>Gets or sets the image height</summary>
    public int? Height { get; init; }

    /// <summary>Gets or sets the loading strategy</summary>
    public string? Loading { get; init; }

    /// <summary>Gets or sets CSS classes</summary>
    public string? Class { get; init; }

    /// <summary>Gets or sets image filters</summary>
    public ImageFilters? Filters { get; init; }

    /// <summary>Gets or sets srcset values</summary>
    public IEnumerable<ImageSrcSet>? SrcSet { get; init; }

    /// <summary>Gets or sets sizes attribute values</summary>
    public IEnumerable<string>? Sizes { get; init; }
}

/// <summary>
/// Supported image formats
/// </summary>
public enum ImageFormat
{
    /// <summary>WebP format</summary>
    WebP,
    
    /// <summary>PNG format</summary>
    PNG,
    
    /// <summary>JPEG format</summary>
    JPEG
}

/// <summary>
/// Defines a srcset entry with width and pixel density
/// </summary>
public record ImageSrcSet
{
    /// <summary>Gets or sets the width in pixels</summary>
    public int Width { get; init; }
    
    /// <summary>Gets or sets the pixel density (e.g., 2 for 2x)</summary>
    public int? PixelDensity { get; init; }
}