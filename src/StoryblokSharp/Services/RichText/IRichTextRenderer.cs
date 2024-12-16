using StoryblokSharp.Models.RichText;
using StoryblokSharp.Services.RichText;

/// <summary>
/// Interface for rich text rendering functionality
/// </summary>
public interface IRichTextRenderer
{
    /// <summary>
    /// Renders rich text content to HTML
    /// </summary>
    string Render(RichTextContent content, RenderOptions? options = null);

    /// <summary>
    /// Adds a custom resolver for a node type
    /// </summary>
    void AddNode(string key, Func<Node, object> schema);

    /// <summary>
    /// Adds a custom resolver for a mark type
    /// </summary>
    void AddMark(string key, Func<Node, object> schema);

    /// <summary>
    /// Registers a custom component type
    /// </summary>
    void RegisterComponent(string componentType, Type componentClass);

    /// <summary>
    /// Adds a custom component resolver
    /// </summary>
    void AddComponentResolver(IComponentResolver resolver);
}