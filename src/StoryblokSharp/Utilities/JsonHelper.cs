using System.Text.Json;
using System.Text.Json.Serialization;
using StoryblokSharp.Models.Json;

namespace StoryblokSharp.Utilities;

/// <summary>
/// Helper methods for JSON operations
/// </summary>
public static class JsonHelper
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
            new ObjectToInferredTypesConverter()
        }
    };

    /// <summary>
    /// Gets the default JSON serialization options
    /// </summary>
    public static JsonSerializerOptions GetDefaultOptions() => DefaultOptions;

    /// <summary>
    /// Deserializes JSON to a strongly-typed object
    /// </summary>
    public static T? Deserialize<T>(string json, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Deserialize<T>(json, options ?? DefaultOptions);
    }

    /// <summary>
    /// Serializes an object to JSON
    /// </summary>
    public static string Serialize<T>(T value, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Serialize(value, options ?? DefaultOptions);
    }

    /// <summary>
    /// Attempts to parse a value from a dynamic object
    /// </summary>
    public static T? TryGetValue<T>(object? obj, string propertyName)
    {
        if (obj is IDictionary<string, object> dict && 
            dict.TryGetValue(propertyName, out var value))
        {
            try
            {
                if (value is JsonElement element)
                {
                    return element.Deserialize<T>();
                }
                return (T?)value;
            }
            catch
            {
                return default;
            }
        }
        return default;
    }
}