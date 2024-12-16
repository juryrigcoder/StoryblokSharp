using Microsoft.Extensions.Options;
using StoryblokSharp.Models.RichText;
using StoryblokSharp.Utilities.RichText;

namespace StoryblokSharp.Services.RichText.NodeResolvers;

/// <summary>
/// Resolves Image nodes to HTML, including optimization options
/// </summary>
public sealed class ImageNodeResolver : INodeResolver
{
    private readonly IHtmlUtilities _htmlUtils;
    private readonly IAttributeUtilities _attrUtils;
    private readonly StringBuilderCache _builderCache;
    private readonly RichTextOptions _options;
    private int _keyCounter;

    public ImageNodeResolver(
        IHtmlUtilities htmlUtils,
        IAttributeUtilities attrUtils,
        StringBuilderCache builderCache,
        IOptions<RichTextOptions> options)
    {
        _htmlUtils = htmlUtils ?? throw new ArgumentNullException(nameof(htmlUtils));
        _attrUtils = attrUtils ?? throw new ArgumentNullException(nameof(attrUtils));
        _builderCache = builderCache ?? throw new ArgumentNullException(nameof(builderCache));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public string Resolve(IRichTextNode node)
    {
        if (node.Attrs == null || !node.Attrs.TryGetValue("src", out var src))
            return string.Empty;

        var srcString = src.ToString()!;
        var attrs = BuildImageAttributes(srcString, node.Attrs);

        if (_options.KeyedResolvers)
        {
            _keyCounter++;
            attrs["key"] = $"img-{_keyCounter}";
        }

        return $"<img {_attrUtils.FormatAttributes(attrs)}>";
    }

    private Dictionary<string, string> BuildImageAttributes(string src, IDictionary<string, object> nodeAttrs)
    {
        var attrs = new Dictionary<string, string>
        {
            ["src"] = _options.OptimizeImages ? OptimizeImageSrc(src) : src
        };

        // Add alt and title if present
        if (nodeAttrs.TryGetValue("alt", out var alt))
            attrs["alt"] = alt.ToString()!;
        if (nodeAttrs.TryGetValue("title", out var title))
            attrs["title"] = title.ToString()!;

        if (_options.OptimizeImages && _options.ImageOptions != null)
        {
            var options = _options.ImageOptions;

            // Add width and height
            if (options.Width.HasValue)
                attrs["width"] = options.Width.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
            if (options.Height.HasValue)
                attrs["height"] = options.Height.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);

            // Add loading attribute
            if (!string.IsNullOrEmpty(options.Loading))
                attrs["loading"] = options.Loading;

            // Add CSS class
            if (!string.IsNullOrEmpty(options.Class))
                attrs["class"] = options.Class;

            // Add srcset if defined and source is from Storyblok CDN
            if (src.Contains("//a.storyblok.com/") && options.SrcSet?.Any() == true)
            {
                var srcset = new List<string>();
                foreach (var size in options.SrcSet)
                {
                    var optimizedSrc = OptimizeImageSrc(src, size.Width, size.PixelDensity);
                    srcset.Add($"{optimizedSrc} {size.Width}w");
                }
                attrs["srcset"] = string.Join(", ", srcset);
            }

            // Add sizes if defined
            if (options.Sizes?.Any() == true)
            {
                attrs["sizes"] = string.Join(", ", options.Sizes);
            }
        }

        return attrs;
    }

    private string OptimizeImageSrc(string src, int? width = null, int? pixelDensity = null)
    {
        if (!src.Contains("//a.storyblok.com/"))
            return src;

        var sb = _builderCache.Acquire();
        try
        {
            sb.Append(src).Append("/m/");

            // Add dimensions if specified
            var w = width ?? _options.ImageOptions?.Width;
            var h = width != null ? 0 : _options.ImageOptions?.Height;
            
            if (w.HasValue || h.HasValue)
            {
                var scale = pixelDensity ?? 1;
                sb.Append(System.Globalization.CultureInfo.InvariantCulture, $"{(w ?? 0) * scale}x{(h ?? 0) * scale}/");
            }

            // Add filters if specified
            if (_options.ImageOptions?.Filters != null)
            {
                var filters = new List<string>();
                var f = _options.ImageOptions.Filters;

                if (f.Quality.HasValue)
                    filters.Add($"quality({f.Quality.Value})");
                if (!string.IsNullOrEmpty(f.Format))
                    filters.Add($"format({f.Format.ToLower(System.Globalization.CultureInfo.InvariantCulture)})");
                if (f.Grayscale == true)
                    filters.Add("grayscale()");
                if (f.Blur.HasValue)
                    filters.Add($"blur({f.Blur.Value})");
                if (f.Brightness.HasValue)
                    filters.Add($"brightness({f.Brightness.Value})");
                if (f.Rotate.HasValue)
                    filters.Add($"rotate({f.Rotate.Value})");

                if (filters.Count > 0)
                    sb.Append(System.Globalization.CultureInfo.InvariantCulture, $"filters:{string.Join(":", filters)}");
            }

            return sb.ToString();
        }
        finally
        {
            _builderCache.Release(sb);
        }
    }
}