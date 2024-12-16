using Microsoft.Extensions.Options;
using StoryblokSharp.Models.RichText;
using StoryblokSharp.Utilities.RichText;

namespace StoryblokSharp.Services.RichText.NodeResolvers;

/// <summary>
/// Resolves text nodes to HTML, including any applied marks
/// </summary>
public sealed class TextNodeResolver : INodeResolver
{
    private readonly IHtmlUtilities _htmlUtils;
    private readonly MarkNodeResolver _markResolver;
    private readonly RichTextOptions _options;

    public TextNodeResolver(
        IHtmlUtilities htmlUtils,
        MarkNodeResolver markResolver,
        IOptions<RichTextOptions> options)
    {
        _htmlUtils = htmlUtils ?? throw new ArgumentNullException(nameof(htmlUtils));
        _markResolver = markResolver ?? throw new ArgumentNullException(nameof(markResolver));
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public string Resolve(IRichTextNode node)
    {
        if (node is TextNode textNode && textNode.Marks?.Any() == true)
        {
            var text = _htmlUtils.Encode(textNode.Text ?? string.Empty);
            return textNode.Marks.Aggregate(text, (acc, mark) =>
                _markResolver.Resolve(new MarkNode
                {
                    Type = mark.Type,
                    MarkType = mark.MarkType,
                    Attrs = mark.Attrs,
                    Text = acc
                }));
        }
        return _htmlUtils.Encode(node.Text ?? string.Empty);
    }
}