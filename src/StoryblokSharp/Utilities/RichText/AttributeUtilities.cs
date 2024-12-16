using System.Text.RegularExpressions;

namespace StoryblokSharp.Utilities.RichText;

/// <summary>
/// Provides utilities for handling HTML attributes
/// </summary>
public sealed class AttributeUtilities : IAttributeUtilities
{
    private readonly StringBuilderCache _builderCache;
    private readonly IHtmlUtilities _htmlUtils;

    // Regex for style property validation
    private static readonly Regex StylePropertyRegex = new(@"^[a-zA-Z0-9\-]+$", RegexOptions.Compiled);
    private static readonly Regex StyleValueRegex = new(@"^[a-zA-Z0-9\-\s\.,#%()]+$", RegexOptions.Compiled);
    
    // Common style properties that don't need validation
    private static readonly HashSet<string> SafeStyleProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "color", "background-color", "font-size", "font-weight", "font-style",
        "text-align", "text-decoration", "margin", "padding", "border",
        "display", "width", "height", "min-width", "min-height",
        "max-width", "max-height", "line-height", "border-radius"
    };

    public AttributeUtilities(StringBuilderCache builderCache, IHtmlUtilities htmlUtils)
    {
        _builderCache = builderCache ?? throw new ArgumentNullException(nameof(builderCache));
        _htmlUtils = htmlUtils ?? throw new ArgumentNullException(nameof(htmlUtils));
    }

    /// <summary>
    /// Formats attributes into a string suitable for HTML output
    /// </summary>
    public string FormatAttributes(IDictionary<string, string>? attrs)
    {
        if (attrs == null || attrs.Count == 0) return string.Empty;

        var sb = _builderCache.Acquire();
        try
        {
            var first = true;
            foreach (var attr in attrs)
            {
                if (string.IsNullOrWhiteSpace(attr.Key)) continue;
                if (!first) sb.Append(' ');
                first = false;

                sb.Append(SanitizeAttributeName(attr.Key))
                  .Append("=\"")
                  .Append(_htmlUtils.EncodeAttribute(attr.Value))
                  .Append('"');
            }
            return sb.ToString();
        }
        finally
        {
            _builderCache.Release(sb);
        }
    }

    /// <summary>
    /// Builds a style string from a dictionary of style attributes
    /// </summary>
    public string BuildStyleString(IDictionary<string, object>? attrs)
    {
        if (attrs == null || attrs.Count == 0) return string.Empty;

        var sb = _builderCache.Acquire();
        try
        {
            var first = true;
            foreach (var attr in attrs)
            {
                var name = attr.Key;
                var value = attr.Value?.ToString();

                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(value)) continue;
                if (!IsValidStyleProperty(name, value)) continue;

                if (!first) sb.Append("; ");
                first = false;

                sb.Append(name.ToLowerInvariant())
                  .Append(": ")
                  .Append(value);
            }
            return sb.ToString();
        }
        finally
        {
            _builderCache.Release(sb);
        }
    }

    /// <summary>
    /// Merges multiple attribute dictionaries, with later values overriding earlier ones
    /// </summary>
    public Dictionary<string, string> MergeAttributes(params IDictionary<string, string>?[] attrSets)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        
        foreach (var attrs in attrSets)
        {
            if (attrs == null) continue;
            
            foreach (var attr in attrs)
            {
                if (string.IsNullOrWhiteSpace(attr.Key)) continue;
                result[attr.Key] = attr.Value;
            }
        }
        
        return result;
    }

    /// <summary>
    /// Merges HTML attributes with object attributes
    /// </summary>
    public Dictionary<string, string> MergeAttributes(IDictionary<string, string>? htmlAttrs, IDictionary<string, object>? objAttrs)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // Copy HTML attributes
        if (htmlAttrs != null)
        {
            foreach (var attr in htmlAttrs)
            {
                if (string.IsNullOrWhiteSpace(attr.Key)) continue;
                result[attr.Key] = attr.Value;
            }
        }

        // Convert and copy object attributes
        if (objAttrs != null)
        {
            foreach (var attr in objAttrs)
            {
                if (string.IsNullOrWhiteSpace(attr.Key)) continue;
                result[attr.Key] = attr.Value.ToString()!;
            }
        }

        return result;
    }

    /// <summary>
    /// Sanitizes an attribute name by removing invalid characters
    /// </summary>
    private static string SanitizeAttributeName(string name)
    {
        return new string(name.Where(c => char.IsLetterOrDigit(c) || c == '-').ToArray())
            .ToLowerInvariant();
    }

    /// <summary>
    /// Validates a CSS style property and value
    /// </summary>
    private static bool IsValidStyleProperty(string property, string value)
    {
        // Check if it's a known safe property
        if (SafeStyleProperties.Contains(property))
        {
            return StyleValueRegex.IsMatch(value);
        }

        // Validate custom properties more strictly
        return StylePropertyRegex.IsMatch(property) && StyleValueRegex.IsMatch(value);
    }

    /// <summary>
    /// Extracts style attributes from a style string
    /// </summary>
    public Dictionary<string, string> ParseStyleString(string? style)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        
        if (string.IsNullOrWhiteSpace(style)) return result;

        foreach (var declaration in style.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = declaration.Split(':', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2) continue;

            var name = parts[0].Trim();
            var value = parts[1].Trim();

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(value)) continue;
            if (!IsValidStyleProperty(name, value)) continue;

            result[name] = value;
        }

        return result;
    }
}