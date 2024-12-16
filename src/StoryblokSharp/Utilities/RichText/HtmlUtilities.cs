using System.Collections.Frozen;
using System.Web;
using System.Runtime.CompilerServices;
using System.Text;

namespace StoryblokSharp.Utilities.RichText;

/// <summary>
/// Provides utilities for HTML encoding and string manipulation
/// </summary>
public sealed class HtmlUtilities : IHtmlUtilities
{
    private static readonly FrozenDictionary<char, string> HtmlEscapes = new Dictionary<char, string>
    {
        { '&', "&amp;" },
        { '<', "&lt;" },
        { '>', "&gt;" },
        { '"', "&quot;" },
        { '\'', "&#39;" }
    }.ToFrozenDictionary();

    private readonly StringBuilderCache _builderCache;

    public HtmlUtilities(StringBuilderCache builderCache)
    {
        _builderCache = builderCache ?? throw new ArgumentNullException(nameof(builderCache));
    }

    /// <inheritdoc/>
    public string Encode(string text)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;

        // Quick check if encoding is needed
        var needsEncoding = false;
        foreach (var c in text)
        {
            if (HtmlEscapes.ContainsKey(c))
            {
                needsEncoding = true;
                break;
            }
        }

        if (!needsEncoding) return text;

        // Use StringBuilder for encoding
        var sb = _builderCache.Acquire();
        try
        {
            EncodeToStringBuilder(text, sb);
            return sb.ToString();
        }
        finally
        {
            _builderCache.Release(sb);
        }
    }

    /// <inheritdoc/>
    public string Decode(string html)
    {
        if (string.IsNullOrEmpty(html)) return string.Empty;
        return HttpUtility.HtmlDecode(html);
    }

    /// <inheritdoc/>
    public string EncodeAttribute(string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return HttpUtility.HtmlAttributeEncode(value);
    }

    /// <inheritdoc/>
    public bool IsValidHtml(string html)
    {
        if (string.IsNullOrEmpty(html)) return false;

        var stack = new Stack<string>();
        var tagStart = -1;
        var inTag = false;
        var inQuote = false;
        var quoteChar = '\0';

        for (var i = 0; i < html.Length; i++)
        {
            var c = html[i];

            if (inQuote)
            {
                if (c == quoteChar)
                {
                    inQuote = false;
                }
                continue;
            }

            if (c == '"' || c == '\'')
            {
                if (inTag && !inQuote)
                {
                    inQuote = true;
                    quoteChar = c;
                }
                continue;
            }

            if (c == '<')
            {
                if (inTag) return false; // Nested <
                inTag = true;
                tagStart = i + 1;
                continue;
            }

            if (c == '>')
            {
                if (!inTag) return false; // Unexpected >
                inTag = false;

                if (tagStart >= i)
                {
                    continue;
                }
                var tag = html[tagStart..i];

                if (tag.StartsWith('/'))
                {
                    var closeTag = tag[1..];
                    if (stack.Count == 0 || stack.Pop() != closeTag)
                    {
                        return false; // Mismatched closing tag
                    }
                }
                else if (!tag.EndsWith('/'))
                {
                    if (!IsSelfClosingTag(tag))
                    {
                        stack.Push(GetTagName(tag));
                    }
                }
            }
        }

        return !inTag && !inQuote && stack.Count == 0;
    }

    /// <inheritdoc/>
    public string SanitizeTagName(string tag)
    {
        if (string.IsNullOrEmpty(tag)) return string.Empty;
        
        var sb = _builderCache.Acquire();
        try
        {
            foreach (var c in tag)
            {
                if (IsValidTagChar(c))
                {
                    sb.Append(char.ToLowerInvariant(c));
                }
            }
            return sb.ToString();
        }
        finally
        {
            _builderCache.Release(sb);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void EncodeToStringBuilder(string text, StringBuilder sb)
    {
        foreach (var c in text)
        {
            if (HtmlEscapes.TryGetValue(c, out var escaped))
            {
                sb.Append(escaped);
            }
            else
            {
                sb.Append(c);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsSelfClosingTag(string tag)
    {
        tag = GetTagName(tag);
        return tag is "area" or "base" or "br" or "col" or "embed" or "hr" or
                      "img" or "input" or "link" or "meta" or "param" or "source" or
                      "track" or "wbr";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetTagName(string tag)
    {
        var spaceIndex = tag.IndexOf(' ');
        return spaceIndex >= 0 ? tag[..spaceIndex].ToLowerInvariant() : tag.ToLowerInvariant();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsValidTagChar(char c)
    {
        return char.IsLetterOrDigit(c) || c == '-' || c == '_';
    }
}

/// <summary>
/// Interface for HTML utilities
/// </summary>
public interface IHtmlUtilities
{
    /// <summary>
    /// Encodes text as HTML by converting special characters to entities
    /// </summary>
    string Encode(string text);

    /// <summary>
    /// Decodes HTML entities back to text
    /// </summary>
    string Decode(string html);

    /// <summary>
    /// Encodes text for use in HTML attributes
    /// </summary>
    string EncodeAttribute(string value);

    /// <summary>
    /// Checks if HTML string has valid tag structure
    /// </summary>
    bool IsValidHtml(string html);

    /// <summary>
    /// Sanitizes a tag name by removing invalid characters
    /// </summary>
    string SanitizeTagName(string tag);
}