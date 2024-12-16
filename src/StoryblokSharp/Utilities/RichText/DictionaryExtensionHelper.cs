namespace StoryblokSharp.Utilities.RichText;

/// <summary>
/// Extension methods for dictionary operations
/// </summary>
public static class DictionaryExtensions
{
    /// <summary>
    /// Gets a dictionary value or a default if not found
    /// </summary>
    public static T? GetValueOrDefault<T>(this IDictionary<string, object>? dict, string key, T? defaultValue = default)
    {
        if (dict == null || !dict.TryGetValue(key, out var value))
            return defaultValue;

        if (value is T typedValue)
            return typedValue;

        try
        {
            // Try to convert the value
            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// Gets a string value from a dictionary or default if not found
    /// </summary>
    public static string GetStringOrDefault(this IDictionary<string, object>? dict, string key, string defaultValue = "")
    {
        if (dict == null || !dict.TryGetValue(key, out var value))
            return defaultValue;

        return value.ToString() ?? defaultValue;
    }

    /// <summary>
    /// Gets an integer value from a dictionary or default if not found
    /// </summary>
    public static int GetIntOrDefault(this IDictionary<string, object>? dict, string key, int defaultValue = 0)
    {
        if (dict == null || !dict.TryGetValue(key, out var value))
            return defaultValue;

        if (value is int intValue)
            return intValue;

        if (int.TryParse(value.ToString(), out var parsedValue))
            return parsedValue;

        return defaultValue;
    }
}