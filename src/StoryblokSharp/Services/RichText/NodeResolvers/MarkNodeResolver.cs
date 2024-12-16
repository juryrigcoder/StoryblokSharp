using Microsoft.Extensions.Options;
using StoryblokSharp.Models.RichText;
using StoryblokSharp.Utilities.RichText;

namespace StoryblokSharp.Services.RichText.NodeResolvers;

/// <summary>
/// Resolves mark nodes (inline formatting) to HTML
/// </summary>

public sealed class MarkNodeResolver : INodeResolver
{
    private readonly IHtmlUtilities _htmlUtils;
    private readonly IAttributeUtilities _attrUtils;
    private readonly RichTextOptions _options;
    private readonly StringBuilderCache _builderCache;

    public MarkNodeResolver(
        IHtmlUtilities htmlUtils,
        IAttributeUtilities attrUtils,
        IOptions<RichTextOptions> options,
        StringBuilderCache builderCache)
    {
        _htmlUtils = htmlUtils ?? throw new ArgumentNullException(nameof(htmlUtils));
        _attrUtils = attrUtils ?? throw new ArgumentNullException(nameof(attrUtils));
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        _builderCache = builderCache ?? throw new ArgumentNullException(nameof(builderCache));
    }

    public string Resolve(IRichTextNode node)
    {
        if (node is not MarkNode markNode)
            return _htmlUtils.Encode(node.Text ?? string.Empty);

        var (tag, attrs) = GetMarkTagAndAttributes(markNode);
        if (tag == null)
            return _htmlUtils.Encode(markNode.Text ?? string.Empty);

        if (_options.KeyedResolvers)
        {
            attrs = attrs ?? new Dictionary<string, string>();
            attrs["key"] = $"{tag}-{GetNextKey()}";
        }

        return WrapInTag(_htmlUtils.Encode(markNode.Text ?? string.Empty), tag, attrs);
    }

    private (string? tag, Dictionary<string, string>? attrs) GetMarkTagAndAttributes(MarkNode mark)
    {
        return mark.MarkType switch
        {
            MarkTypes.Bold or MarkTypes.Strong => ("strong", null),
            MarkTypes.Strike => ("s", null),
            MarkTypes.Underline => ("u", null),
            MarkTypes.Italic => ("em", null),
            MarkTypes.Code => ("code", null),
            MarkTypes.Link => GetLinkTagAndAttributes(mark),
            MarkTypes.Styled => GetStyledTagAndAttributes(mark),
            MarkTypes.Superscript => ("sup", null),
            MarkTypes.Subscript => ("sub", null),
            MarkTypes.TextStyle => GetTextStyleTagAndAttributes(mark),
            MarkTypes.Highlight => ("mark", null),
            _ => (null, null)
        };
    }

private (string tag, Dictionary<string, string> attrs) GetLinkTagAndAttributes(MarkNode mark)
{
    var attrs = new Dictionary<string, string>();
    if (mark.Attrs?.TryGetValue("href", out var href) == true)
    {
        attrs["href"] = href.ToString()!;
    }
    if (mark.Attrs?.TryGetValue("target", out var target) == true)
    {
        attrs["target"] = target.ToString()!;
    }
    return ("a", attrs);
}

    private (string tag, Dictionary<string, string> attrs) GetStyledTagAndAttributes(MarkNode mark)
    {
        var attrs = new Dictionary<string, string>();
        if (mark.Attrs != null)
        {
            var style = _attrUtils.BuildStyleString(mark.Attrs);
            if (!string.IsNullOrEmpty(style))
            {
                attrs["style"] = style;
            }
        }
        return ("span", attrs);
    }

    private (string tag, Dictionary<string, string> attrs) GetTextStyleTagAndAttributes(MarkNode mark)
    {
        var attrs = new Dictionary<string, string>();
        if (mark.Attrs != null)
        {
            if (mark.Attrs.TryGetValue("class", out var className))
            {
                attrs["class"] = className?.ToString() ?? string.Empty;
            }
            var style = _attrUtils.BuildStyleString(mark.Attrs);
            if (!string.IsNullOrEmpty(style))
            {
                attrs["style"] = style;
            }
        }
        return ("span", attrs);
    }

    private string WrapInTag(string content, string tag, IDictionary<string, string>? attrs = null)
    {
        var attrString = attrs != null && attrs.Any() ? " " + _attrUtils.FormatAttributes(attrs) : string.Empty;
        var sb = _builderCache.Acquire();
        try
        {
            sb.Append('<').Append(tag).Append(attrString).Append('>');
            sb.Append(content);
            sb.Append("</").Append(tag).Append('>');
            return sb.ToString();
        }
        finally
        {
            _builderCache.Release(sb);
        }
    }

    private int _keyCounter;
    private int GetNextKey() => Interlocked.Increment(ref _keyCounter);
}