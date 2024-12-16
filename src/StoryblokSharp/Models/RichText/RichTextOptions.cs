using System.Text.Json;
using StoryblokSharp.Models.RichText;

namespace StoryblokSharp.Services.RichText;

/// <summary>
/// Options for rich text rendering
/// </summary>
public class RichTextOptions
{
    /// <summary>
    /// Gets or sets whether images should be optimized
    /// </summary>
    public bool OptimizeImages { get; set; }

    /// <summary>
    /// Gets or sets options for image optimization
    /// </summary>
    public ImageOptimizationOptions? ImageOptions { get; set; }

    /// <summary>
    /// Gets or sets whether to add keys to resolved elements for framework compatibility
    /// </summary>
    public bool KeyedResolvers { get; set; }

    /// <summary>
    /// Gets or sets custom resolvers for specific node types
    /// </summary>
    public Dictionary<string, Func<IRichTextNode, string>>? CustomResolvers { get; set; }

    /// <summary>
    /// Gets or sets the priority order for applying marks
    /// </summary>
    public MarkTypes[]? MarkSortPriority { get; set; }

    /// <summary>
    /// Gets or sets HTML sanitization options
    /// </summary>
    public HtmlSanitizerOptions? SanitizerOptions { get; set; }

    /// <summary>
    /// Gets or sets whether to remove comments from the output
    /// </summary>
    public bool RemoveComments { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to minify the output HTML
    /// </summary>
    public bool MinifyOutput { get; set; }

    /// <summary>
    /// Gets or sets a fallback component resolver
    /// </summary>
    public Func<string, IDictionary<string, object>, string>? ComponentResolver { get; set; }

    /// <summary>
    /// Gets or sets options for component resolution
    /// </summary>
    public ComponentOptions ComponentOptions { get; set; } = new();

    /// <summary>
    /// Gets or sets serialization options for component props
    /// </summary>
    public JsonSerializerOptions? SerializerOptions { get; set; }

    /// <summary>
    /// Gets or sets the strategy for handling invalid nodes
    /// </summary>
    public InvalidNodeStrategy InvalidNodeHandling { get; set; } = InvalidNodeStrategy.Remove;
}

/// <summary>
/// Options for component resolution
/// </summary>
public class ComponentOptions
{
    /// <summary>
    /// Gets or sets whether component resolution is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to validate component props
    /// </summary>
    public bool ValidateProps { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to throw exceptions on component errors
    /// </summary>
    public bool ThrowOnError { get; set; }

    /// <summary>
    /// Gets or sets whether to cache component instances
    /// </summary>
    public bool EnableCaching { get; set; }

    /// <summary>
    /// Gets or sets the cache duration for component instances
    /// </summary>
    public TimeSpan? CacheDuration { get; set; }

    /// <summary>
    /// Gets the registered component mappings
    /// Key: Component type name (e.g., "my-component")
    /// Value: .NET type to instantiate
    /// </summary>
    public Dictionary<string, Type> ComponentMappings { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets custom prop transformers
    /// Key: Component type name
    /// Value: Function to transform props before passing to component
    /// </summary>
    public Dictionary<string, Func<IDictionary<string, object>, IDictionary<string, object>>> PropTransformers { get; set; } 
        = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets a list of allowed component types
    /// If null or empty, all registered components are allowed
    /// </summary>
    public HashSet<string>? AllowedComponents { get; set; }

    /// <summary>
    /// Gets or sets global props to pass to all components
    /// </summary>
    public Dictionary<string, object> GlobalProps { get; set; } = new();
}

/// <summary>
/// Strategy for handling invalid nodes
/// </summary>
public enum InvalidNodeStrategy
{
    /// <summary>
    /// Remove invalid nodes from output
    /// </summary>
    Remove,

    /// <summary>
    /// Replace invalid nodes with empty string
    /// </summary>
    Replace,

    /// <summary>
    /// Keep invalid nodes as-is
    /// </summary>
    Keep,

    /// <summary>
    /// Throw exception on invalid nodes
    /// </summary>
    Throw
}