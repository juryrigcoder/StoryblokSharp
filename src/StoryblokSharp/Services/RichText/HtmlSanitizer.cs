using System.Collections.Frozen;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;

namespace StoryblokSharp.Services.RichText;

/// <summary>
/// Provides HTML sanitization for rich text content
/// </summary>
public sealed class HtmlSanitizer : IHtmlSanitizer
{
    private readonly HtmlSanitizerOptions _options;
    private readonly FrozenSet<string> _allowedTags;
    private readonly FrozenDictionary<string, FrozenSet<string>> _allowedAttributes;
    private readonly FrozenSet<string> _uriAttributes;
    private readonly FrozenSet<string> _selfClosingTags;
    private readonly FrozenSet<string> _allowedProtocols;

    // Cache compiled regexes for performance
    private static readonly Regex TagRegex = new(@"<(?:\/)?([a-zA-Z0-9]+)(?:\s[^>]*)?\/?>", RegexOptions.Compiled);
    private static readonly Regex AttributeRegex = new(@"([a-zA-Z0-9\-]+)(?:\s*=\s*(?:""([^""]*)""|'([^']*)'|([^\s""'=<>`]+)))?", RegexOptions.Compiled);
    private static readonly Regex CommentRegex = new(@"<!--[\s\S]*?-->", RegexOptions.Compiled);
    private static readonly Regex ScriptRegex = new(@"<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex StyleRegex = new(@"<style\b[^<]*(?:(?!<\/style>)<[^<]*)*<\/style>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex ProtocolRegex = new(@"^([a-zA-Z][a-zA-Z0-9+.-]*:|\/\/)", RegexOptions.Compiled);

    public HtmlSanitizer(IOptions<HtmlSanitizerOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options.Value);
        _options = options.Value;

        // Initialize frozen sets for performance
        _allowedTags = _options.AllowedTags.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
        _allowedAttributes = _options.AllowedAttributes.ToFrozenDictionary(
            x => x.Key,
            x => x.Value.ToFrozenSet(StringComparer.OrdinalIgnoreCase),
            StringComparer.OrdinalIgnoreCase
        );
        _uriAttributes = _options.UriAttributes.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
        _selfClosingTags = _options.SelfClosingTags.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
        _allowedProtocols = _options.AllowedProtocols.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
        _selfClosingTags = _options.SelfClosingTags.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc/>
    public string Sanitize(string html)
    {
        if (string.IsNullOrEmpty(html)) return string.Empty;

        // Remove unwanted elements completely
        html = CommentRegex.Replace(html, string.Empty);
        html = ScriptRegex.Replace(html, string.Empty);
        html = StyleRegex.Replace(html, string.Empty);

        // Process remaining HTML
        return TagRegex.Replace(html, match =>
        {
            var tag = match.Groups[1].Value.ToLowerInvariant();
            
            // Check if tag is allowed
            if (!_allowedTags.Contains(tag))
            {
                return string.Empty;
            }

            // Process tag with attributes
            var fullTag = match.Value;
            var sanitizedTag = SanitizeTag(fullTag, tag);
            
            return sanitizedTag;
        });
    }

    private string SanitizeTag(string fullTag, string tagName)
    {
        // Handle self-closing tags
        var isSelfClosing = _selfClosingTags.Contains(tagName);
        var isClosingTag = fullTag.StartsWith("</", StringComparison.Ordinal);

        // For closing tags, just return the clean version
        if (isClosingTag)
        {
            return $"</{tagName}>";
        }

        // Process attributes if present
        var sanitizedAttrs = new List<string>();
        var attributeMatches = AttributeRegex.Matches(fullTag);
        
        foreach (Match attrMatch in attributeMatches)
        {
            var attrName = attrMatch.Groups[1].Value.ToLowerInvariant();
            var attrValue = attrMatch.Groups[2].Value; // Quoted value
            
            if (string.IsNullOrEmpty(attrValue))
            {
                attrValue = attrMatch.Groups[3].Value; // Single quoted value
            }
            if (string.IsNullOrEmpty(attrValue))
            {
                attrValue = attrMatch.Groups[4].Value; // Unquoted value
            }

            // Check if attribute is allowed for this tag
            if (_allowedAttributes.TryGetValue(tagName, out var allowedAttrs) && allowedAttrs.Contains(attrName))
            {
                var sanitizedValue = SanitizeAttributeValue(attrName, attrValue);
                sanitizedAttrs.Add($"{attrName}=\"{sanitizedValue}\"");
            }
            else if (_allowedAttributes.TryGetValue("*", out var globalAttrs) && globalAttrs.Contains(attrName))
            {
                var sanitizedValue = SanitizeAttributeValue(attrName, attrValue);
                sanitizedAttrs.Add($"{attrName}=\"{sanitizedValue}\"");
            }
        }

        // Build sanitized tag
        var attributes = sanitizedAttrs.Count > 0 ? " " + string.Join(" ", sanitizedAttrs) : string.Empty;
        return isSelfClosing ? $"<{tagName}{attributes} />" : $"<{tagName}{attributes}>";
    }

    private string SanitizeAttributeValue(string attrName, string value)
    {
        // For URI attributes, ensure protocol is allowed
        if (_uriAttributes.Contains(attrName))
        {
            var match = ProtocolRegex.Match(value);
            if (match.Success)
            {
                var protocol = match.Groups[1].Value.TrimEnd(':', '/');
                if (!_options.AllowedProtocols.Contains(protocol, StringComparer.OrdinalIgnoreCase))
                {
                    return string.Empty;
                }
            }
        }

        // Encode attribute value
        return _options.AttributeEncodingDelegate?.Invoke(value) 
            ?? System.Net.WebUtility.HtmlEncode(value);
    }
}

// Rest of the code remains the same...
/// <summary>
/// Options for HTML sanitization
/// </summary>
public class HtmlSanitizerOptions
{
    /// <summary>
    /// Gets or sets the allowed HTML tags
    /// </summary>
    public HashSet<string> AllowedTags { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "p", "br", "b", "i", "u", "em", "strong", "a", "h1", "h2", "h3", "h4", "h5", "h6",
        "blockquote", "code", "pre", "hr", "kbd", "mark", "s", "sub", "sup",
        "ul", "ol", "li", "table", "thead", "tbody", "tr", "th", "td",
        "img", "figure", "figcaption", "picture", "source"
    };

    /// <summary>
    /// Gets or sets the allowed attributes for specific tags
    /// </summary>
    public Dictionary<string, HashSet<string>> AllowedAttributes { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        ["*"] = new(StringComparer.OrdinalIgnoreCase) { "class", "id", "title" },
        ["a"] = new(StringComparer.OrdinalIgnoreCase) { "href", "target", "rel", "download", "uuid" },
        ["img"] = new(StringComparer.OrdinalIgnoreCase) { "src", "alt", "width", "height", "loading", "srcset", "sizes" },
        ["source"] = new(StringComparer.OrdinalIgnoreCase) { "src", "srcset", "type", "media" }
    };

    /// <summary>
    /// Gets or sets attributes that contain URIs
    /// </summary>
    public HashSet<string> UriAttributes { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "href",
        "src",
        "srcset",
        "cite",
        "action",
        "data"
    };

    /// <summary>
    /// Gets or sets allowed URI protocols
    /// </summary>
    public HashSet<string> AllowedProtocols { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "http",
        "https",
        "mailto",
        "tel",
        "data"
    };

    /// <summary>
    /// Gets or sets self-closing tags
    /// </summary>
    public HashSet<string> SelfClosingTags { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "area", "base", "br", "col", "embed", "hr", "img", "input", 
        "link", "meta", "param", "source", "track", "wbr"
    };

    /// <summary>
    /// Gets or sets a delegate for attribute value encoding
    /// </summary>
    public Func<string, string>? AttributeEncodingDelegate { get; private set; }
}

/// <summary>
/// Interface for HTML sanitization
/// </summary>
public interface IHtmlSanitizer
{
    /// <summary>
    /// Sanitizes HTML content by removing disallowed tags and attributes
    /// </summary>
    /// <param name="html">The HTML to sanitize</param>
    /// <returns>Sanitized HTML</returns>
    string Sanitize(string html);
}