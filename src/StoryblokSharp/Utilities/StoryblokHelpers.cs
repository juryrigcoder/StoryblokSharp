using StoryblokSharp.Models.Stories;

namespace StoryblokSharp.Utilities;

/// <summary>
/// Helper methods for Storyblok operations
/// </summary>
public class StoryblokHelpers
{
    /// <summary>
    /// Checks if a URL is a CDN URL
    /// </summary>
    public static bool IsCDNUrl(string? url = null) => 
        !string.IsNullOrEmpty(url) && url.Contains("/cdn/");

    /// <summary>
    /// Gets paged options for fetching stories
    /// </summary>
    public static StoryQueryParameters GetOptionsPage(
        StoryQueryParameters options, 
        int perPage = 25, 
        int page = 1)
    {
        return options with
        {
            PerPage = perPage,
            Page = page
        };
    }

    /// <summary>
    /// Creates a range of numbers
    /// </summary>
    public static IEnumerable<int> Range(int start, int end)
    {
        return Enumerable.Range(start, Math.Abs(end - start));
    }

    /// <summary>
    /// Flattens and maps an array of results
    /// </summary>
    public static IEnumerable<T> FlatMap<T, TResult>(
        IEnumerable<TResult> array,
        Func<TResult, IEnumerable<T>> selector)
    {
        return array.SelectMany(selector);
    }

    /// <summary>
    /// Converts query parameters to a URL query string
    /// </summary>
    public static string Stringify(object parameters, string? prefix = null, bool isArray = false)
    {
        var pairs = new List<string>();
        var props = parameters.GetType().GetProperties();

        foreach (var prop in props)
        {
            var value = prop.GetValue(parameters);
            if (value == null) continue;

            var key = isArray ? "" : Uri.EscapeDataString(prop.Name);
            string pair;

            if (value.GetType().IsClass && value.GetType() != typeof(string))
            {
                pair = Stringify(
                    value,
                    prefix != null ? $"{prefix}{Uri.EscapeDataString($"[{key}]")}" : key,
                    value.GetType().IsArray);
            }
            else
            {
                pair = $"{(prefix != null ? $"{prefix}{Uri.EscapeDataString($"[{key}]")}" : key)}={Uri.EscapeDataString(value.ToString() ?? "")}";
            }

            pairs.Add(pair);
        }

        return string.Join("&", pairs);
    }
}