using Microsoft.Extensions.Options;
using StoryblokSharp.Models.RichText;
using StoryblokSharp.Utilities.RichText;

namespace StoryblokSharp.Services.RichText.NodeResolvers;

public class BlockNodeResolver : INodeResolver
{
    private readonly IHtmlUtilities _htmlUtils;
    private readonly StringBuilderCache _builderCache;
    private readonly IAttributeUtilities _attrUtils;
    private readonly RichTextOptions _options;
    private readonly TextNodeResolver _textResolver;
    private readonly ImageNodeResolver _imageResolver;
    private readonly MarkNodeResolver _markResolver;
    private int _keyCounter;

    public BlockNodeResolver(
        IHtmlUtilities htmlUtils,
        StringBuilderCache builderCache,
        IAttributeUtilities attrUtils,
        IOptions<RichTextOptions> options,
        TextNodeResolver textResolver,
        ImageNodeResolver imageResolver,
        MarkNodeResolver markResolver)
    {
        _htmlUtils = htmlUtils ?? throw new ArgumentNullException(nameof(htmlUtils));
        _builderCache = builderCache ?? throw new ArgumentNullException(nameof(builderCache));
        _attrUtils = attrUtils ?? throw new ArgumentNullException(nameof(attrUtils));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _textResolver = textResolver ?? throw new ArgumentNullException(nameof(textResolver));
        _imageResolver = imageResolver ?? throw new ArgumentNullException(nameof(imageResolver));
        _markResolver = markResolver ?? throw new ArgumentNullException(nameof(markResolver));
    }

public string Resolve(IRichTextNode node)
{
    // Special case for empty nodes
    if (node.Content == null && string.IsNullOrEmpty(node.Text))
    {
        return string.Empty;
    }

    // Handle special node types first
    if (node.Type == nameof(BlockTypes.Image).ToLower())
    {
        return _imageResolver.Resolve(node);
    }

    if (node.Type == nameof(TextTypes.Text).ToLower())
    {
        // Special handling for text with marks
        if (node is RichTextNode richNode && richNode.Marks?.Any() == true)
        {
            var text = _textResolver.Resolve(node);
            return richNode.Marks.Aggregate(text, (current, mark) =>
                _markResolver.Resolve(mark with { Text = current }));
        }
        return _textResolver.Resolve(node);
    }

    // Resolve all node content first
    var content = ResolveContent(node);

    // Get the appropriate HTML tag
    var tag = DetermineTag(node);
    if (string.IsNullOrEmpty(tag))
    {
        return content;
    }

    // Handle self-closing tags
    if (tag == "hr" || tag == "br")
    {
        return $"<{tag}>";
    }

    // Create attributes for the tag
    var attrs = CreateAttributesWithKey(tag, node.Attrs);

    // Special handling for lists to maintain nesting
    if (tag == "ul" || tag == "ol")
    {
        return WrapInTag(content, tag, attrs);
    }
    else if (tag == "li")
    {
        return WrapInTag(content, tag, attrs);
    }

    return WrapInTag(content, tag, attrs);
}

private string ResolveContent(IRichTextNode node)
{
    if (node.Content == null || !node.Content.Any())
    {
        return node.Text ?? string.Empty;
    }

    var sb = _builderCache.Acquire();
    try
    {
        foreach (var child in node.Content)
        {
            sb.Append(Resolve(child));
        }
        return sb.ToString();
    }
    finally
    {
        _builderCache.Release(sb);
    }
}

    private string? DetermineTag(IRichTextNode node)
    {
        return node.Type.ToLowerInvariant() switch
        {
            var t when string.Equals(t, nameof(BlockTypes.Document), StringComparison.OrdinalIgnoreCase) => null,
            var t when string.Equals(t, nameof(BlockTypes.Paragraph), StringComparison.OrdinalIgnoreCase) => "p",
            var t when string.Equals(t, nameof(BlockTypes.BulletList), StringComparison.OrdinalIgnoreCase) => "ul",
            var t when string.Equals(t, nameof(BlockTypes.OrderedList), StringComparison.OrdinalIgnoreCase) => "ol",
            var t when string.Equals(t, nameof(BlockTypes.ListItem), StringComparison.OrdinalIgnoreCase) => "li",
            var t when string.Equals(t, nameof(BlockTypes.Quote), StringComparison.OrdinalIgnoreCase) => "blockquote",
            var t when string.Equals(t, nameof(BlockTypes.CodeBlock), StringComparison.OrdinalIgnoreCase) => "pre",
            var t when string.Equals(t, nameof(BlockTypes.Heading), StringComparison.OrdinalIgnoreCase) => $"h{(node.Attrs?.TryGetValue("level", out var level) == true ? level : 1)}",
            var t when string.Equals(t, nameof(BlockTypes.HorizontalRule), StringComparison.OrdinalIgnoreCase) => "hr",
            var t when string.Equals(t, nameof(BlockTypes.HardBreak), StringComparison.OrdinalIgnoreCase) => "br",
            _ => null
        };
    }


    private Dictionary<string, string>? CreateAttributesWithKey(string tag, IDictionary<string, object>? attrs = null)
    {
        Dictionary<string, string>? result = _options.KeyedResolvers ? 
            new Dictionary<string, string> { ["key"] = $"{tag}-{GetNextKey()}" } : 
            null;

        if (attrs != null)
        {
            result ??= new Dictionary<string, string>();
            foreach (var attr in attrs)
            {
                // Skip level attribute for headings since it's used to determine the tag
                if (tag.StartsWith('h') && attr.Key == "level") continue;

                if (attr.Value != null)
                {
                    result[attr.Key] = attr.Value.ToString() ?? string.Empty;
                }
            }
        }

        return result;
    }

    private string WrapInTag(string content, string tag, IDictionary<string, string>? attrs = null)
    {
        var sb = _builderCache.Acquire();
        try
        {
            sb.Append('<').Append(tag);
            
            if (attrs != null && attrs.Any())
            {
                sb.Append(' ').Append(_attrUtils.FormatAttributes(attrs));
            }
            
            sb.Append('>');
            sb.Append(content);
            sb.Append("</").Append(tag).Append('>');
            
            return sb.ToString();
        }
        finally
        {
            _builderCache.Release(sb);
        }
    }
    private int GetNextKey() => Interlocked.Increment(ref _keyCounter);
}