using StoryblokSharp.Models.RichText;

namespace StoryblokSharp.Services.RichText.NodeResolvers;

/// <summary>
/// Interface for resolving rich text nodes to HTML
/// </summary>
public interface INodeResolver
{
    /// <summary>
    /// Resolves a rich text node to its HTML representation
    /// </summary>
    /// <param name="node">The node to resolve</param>
    /// <returns>The HTML string</returns>
    string Resolve(IRichTextNode node);
}