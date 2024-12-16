using StoryblokSharp.Models.RichText;
using StoryblokSharp.Utilities.RichText;

namespace StoryblokSharp.Services.RichText.NodeResolvers;
public class EmojiResolver : INodeResolver 
{
    private readonly IAttributeUtilities _attrUtils;

    public EmojiResolver(IAttributeUtilities attrUtils)
    {
        _attrUtils = attrUtils ?? throw new ArgumentNullException(nameof(attrUtils));
    }

    public string Resolve(IRichTextNode node)
    {
        if (node.Attrs == null || !node.Attrs.TryGetValue("emoji", out var emoji))
            return string.Empty;

        var attrs = new Dictionary<string, string>
        {
            ["data-type"] = "emoji",
            ["data-name"] = node.Attrs.GetValueOrDefault("name", string.Empty)?.ToString() ?? string.Empty,
            ["data-emoji"] = emoji.ToString() ?? string.Empty
        };

        var imgAttrs = new Dictionary<string, string>
        {
            ["src"] = node.Attrs.GetValueOrDefault("fallbackImage", string.Empty)?.ToString() ?? string.Empty,
            ["alt"] = node.Attrs.GetValueOrDefault("name", string.Empty)?.ToString() ?? string.Empty,
            ["style"] = "width: 1.25em; height: 1.25em; vertical-align: text-top",
            ["draggable"] = "false",
            ["loading"] = "lazy"
        };

        var img = $"<img {_attrUtils.FormatAttributes(imgAttrs)}>";
        return $"<span {_attrUtils.FormatAttributes(attrs)}>{img}</span>";
    }
}